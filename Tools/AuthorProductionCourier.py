"""Reproducible garment/equipment authoring on licensed CC0 humanoid anatomy.
Run with Blender 4.5 LTS --background --factory-startup --python this_file.
No runtime primitives, cloth simulation, downloaded paid content, or root motion.
"""
import bpy, bmesh, math, os, json, random
from mathutils import Vector
from mathutils.kdtree import KDTree
from array import array

ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'..'))
OUT=os.path.join(ROOT,'UnityProject/Assets/Shiftbound/ProductionUpgrade/Courier')
os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=os.path.join(ROOT,'UnityProject/Assets/Shiftbound/ThirdParty/Quaternius/Character/Superhero_Male_FullBody.fbx'))
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
body=bpy.data.objects['SuperHero_Male']
# Normalize transforms once. All authored surfaces use metre coordinates.
for o in list(bpy.context.scene.objects):
    bpy.context.view_layer.objects.active=o; o.select_set(True)
    if o.type=='MESH': bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    o.select_set(False)
skin=bpy.data.materials.new('CourierSkin'); skin.diffuse_color=(.64,.36,.22,1)
gear=bpy.data.materials.new('CourierTextiles'); gear.diffuse_color=(1,.55,.06,1)
eyes=bpy.data.materials.new('CourierEyes'); eyes.diffuse_color=(.09,.045,.02,1)
hairmat=bpy.data.materials.new('CourierHair'); hairmat.diffuse_color=(.06,.025,.015,1)
materials=[skin,gear,eyes,hairmat]
body.data.materials.clear(); body.data.materials.append(skin)
for o in list(bpy.context.scene.objects):
    if o.type=='MESH' and o!=body:
        o.data.materials.clear(); o.data.materials.append(eyes if o.name=='Eyes' else hairmat)
        for p in o.data.polygons:p.material_index=0

# Nearest source skin weights transferred to garment surfaces and seams.
points=[body.matrix_world @ v.co for v in body.data.vertices]
tree=KDTree(len(points))
for i,p in enumerate(points):tree.insert(p,i)
tree.balance()
weights=[[(body.vertex_groups[g.group].name,g.weight) for g in v.groups] for v in body.data.vertices]
def skin_object(o, rigid=None):
    o.parent=rig
    o.matrix_parent_inverse=rig.matrix_world.inverted()
    modifier=o.modifiers.new('Courier skeleton','ARMATURE'); modifier.object=rig
    names=[rigid] if rigid else list(body.vertex_groups.keys())
    for name in names:o.vertex_groups.new(name=name)
    for v in o.data.vertices:
        if rigid:o.vertex_groups[rigid].add([v.index],1,'REPLACE')
        else:
            _,index,_=tree.find(o.matrix_world @ v.co)
            for name,w in weights[index]:o.vertex_groups[name].add([v.index],w,'REPLACE')

def atlas_uv(o,patch):
    uv=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
    for p in o.data.polygons:
        for li in p.loop_indices:
            v=o.data.vertices[o.data.loops[li].vertex_index].co
            # Physical weave scale, consistent across seams/garments.
            uv.data[li].uv=((patch%4+.05+(v.x*3.1%1)*.9)/4,(patch//4+.05+(v.z*3.1%1)*.9)/4)

def surface(name,predicate,offset,patch):
    obj=body.copy(); obj.data=body.data.copy(); bpy.context.collection.objects.link(obj); obj.name=name
    bm=bmesh.new(); bm.from_mesh(obj.data)
    remove=[f for f in bm.faces if not predicate(body.matrix_world @ f.calc_center_median())]
    bmesh.ops.delete(bm,geom=remove,context='FACES')
    # Relax anatomy beneath clothing, retaining topology and deformation weights.
    for i in range(5):bmesh.ops.smooth_vert(bm,verts=list(bm.verts),factor=.35,use_axis_x=True,use_axis_y=True,use_axis_z=True)
    bm.normal_update()
    for v in bm.verts:
        world=obj.matrix_world @ v.co
        amount=offset
        if name=='Technical jacket':
            amount+=.006*math.sin(world.z*52+world.x*18)*math.exp(-((world.z-1.1)/.24)**2)
        if name=='Tapered trousers':
            amount+=.006*math.sin(world.z*63+world.x*20)*math.exp(-((world.z-.57)/.12)**2)
        v.co+=v.normal*amount
    bm.to_mesh(obj.data);bm.free()
    obj.data.materials.clear();obj.data.materials.append(gear)
    for p in obj.data.polygons:p.material_index=0;p.use_smooth=True
    atlas_uv(obj,patch)
    return obj

jacket=surface('Technical jacket',lambda p:1.03<p.z<1.56 and abs(p.x)<.71 and not(abs(p.x)<.075 and p.z>1.50),.031,0)
pants=surface('Tapered trousers',lambda p:.17<p.z<1.045,.025,1)
shoe=surface('Trail shoes',lambda p:p.z<.18,.012,4)
# Remove covered anatomy rather than paying for hidden meshes or risking z-fighting.
bm=bmesh.new();bm.from_mesh(body.data)
bmesh.ops.delete(bm,geom=[f for f in bm.faces if (lambda p:p.z<1.555 and abs(p.x)<.705 and not(abs(p.x)<.075 and p.z>1.50))(body.matrix_world @ f.calc_center_median())],context='FACES')
bm.to_mesh(body.data);bm.free()
for p in body.data.polygons:p.use_smooth=True

def mesh(name,verts,faces,patch=4,rigid=None):
    m=bpy.data.meshes.new(name);m.from_pydata(verts,[],faces);m.update()
    o=bpy.data.objects.new(name,m);bpy.context.collection.objects.link(o);m.materials.append(gear)
    for p in m.polygons:p.use_smooth=True
    atlas_uv(o,patch);skin_object(o,rigid)
    return o

def loft(name,rings,patch,rigid=None,n=24,cx=0):
    # Elliptical continuous rings: z, width radius, depth radius, center y, exponent.
    verts=[];faces=[]
    for z,rx,ry,cy,power in rings:
        for i in range(n):
            a=2*math.pi*i/n;c=math.cos(a);s=math.sin(a)
            verts.append((cx+math.copysign(abs(c)**power,c)*rx,cy+math.copysign(abs(s)**power,s)*ry,z))
    for row in range(len(rings)-1):
        for i in range(n):j=(i+1)%n;faces.append((row*n+i,row*n+j,(row+1)*n+j,(row+1)*n+i))
    return mesh(name,verts,faces,patch,rigid)

loft('Ribbed jacket hem',[(1.025,.163,.128,-.035,.85),(1.038,.173,.132,-.035,.85),(1.056,.171,.130,-.035,.85),(1.069,.168,.128,-.035,.85)],4)
loft('Cyan raised collar',[(1.47,.108,.095,-.037,1),(1.495,.103,.089,-.037,1),(1.515,.093,.080,-.037,1),(1.53,.091,.078,-.037,1)],2, 'neck_01')
for s,side in [(-1,'l'),(1,'r')]:
    loft('Trail sole '+side,[(.012,.062,.137,-.014,.65),(.026,.064,.139,-.014,.65),(.045,.061,.137,-.014,.65)],6,'foot_'+side,32,cx=s*.1143)
    loft('Trouser ankle binding '+side,[(.168,.052,.059,-.063,1),(.179,.054,.060,-.063,1),(.193,.054,.060,-.063,1)],4,'calf_'+side,24,cx=s*.1143)
# Soft padded hood resting behind the collar; an open annulus, not a solid torus.
verts=[];faces=[]
for row in range(5):
    t=row/4
    for i in range(25):
        a=math.pi*.04+math.pi*1.92*i/24
        verts.append((math.sin(a)*(.112+.018*math.sin(t*math.pi)), -.057+math.cos(a)*(.112+.025*math.sin(t*math.pi)),1.47-.105*t+.012*math.cos(a)))
for r in range(4):
    for i in range(24):a=r*25+i;faces.append((a,a+1,a+26,a+25))
mesh('Folded storm hood',verts,faces,0,'spine_03')

def ribbon(name,points,width,patch=4,rigid=None,normal=(0,1,0)):
    pts=[Vector(p) for p in points];verts=[];faces=[]
    for i,p in enumerate(pts):
        tangent=pts[min(i+1,len(pts)-1)]-pts[max(0,i-1)]
        side=tangent.cross(Vector(normal)).normalized()*width*.5
        verts.extend([p-side,p+side])
    for i in range(len(pts)-1):faces.append((i*2,i*2+1,i*2+3,i*2+2))
    return mesh(name,verts,faces,patch,rigid)

# Zipper, tailored panel boundaries and cuff seams follow the garment, not torso boxes.
ribbon('Central weatherproof zipper',[(0,.095,1.055),(0,.133,1.19),(0,.123,1.33),(0,.075,1.46)],.009,4)
for s in [-1,1]:
    ribbon('Raglan reflective seam',[(s*.03,.073,1.46),(s*.115,.084,1.435),(s*.21,.07,1.40),(s*.29,.034,1.40)],.007,7)
    ribbon('Diagonal pocket zip',[(s*.067,.113,1.15),(s*.112,.108,1.21),(s*.14,.096,1.24)],.011,4)
    ribbon('Bag shoulder webbing',[(s*.125,-.169,1.14),(s*.14,-.157,1.30),(s*.15,-.139,1.43),(s*.145,-.03,1.50),(s*.14,.073,1.40),(s*.113,.131,1.28),(s*.09,.109,1.16)],.036,4)
    ribbon('Trouser outer seam',[(s*.173,-.018,.98),(s*.184,-.01,.82),(s*.153,-.026,.60),(s*.153,-.045,.43),(s*.138,-.06,.22)],.008,2)

# Delivery bag: tapered sewn shell, depth, rolled upper lip and overlapping flap.
loft('Sewn delivery bag',[(1.085,.12,.045,-.208,.48),(1.10,.147,.067,-.214,.48),(1.16,.157,.070,-.214,.48),(1.31,.157,.073,-.214,.48),(1.39,.148,.060,-.207,.48),(1.41,.125,.040,-.20,.48)],3,'spine_03',32)
flap=[]
for z,w,d in [(1.395,.135,-.278),(1.375,.147,-.287),(1.29,.147,-.297),(1.27,.13,-.30)]:
    flap.extend([(-w,d+.012,z),(0,d-.004,z),(w,d+.012,z)])
mesh('Overlapping bag flap',flap,[(r*3+i,r*3+i+1,(r+1)*3+i+1,(r+1)*3+i) for r in range(3) for i in range(2)],4,'spine_03')
for s in [-1,1]:
    ribbon('Bag bound seam',[(s*.133,-.271,1.11),(s*.148,-.278,1.20),(s*.148,-.279,1.31),(s*.137,-.271,1.39)],.005,7,'spine_03')
    ribbon('Bag retention strap',[(s*.082,-.281,1.40),(s*.082,-.302,1.30),(s*.082,-.29,1.19)],.014,3,'spine_03')
# Original angular leaf/wing courier insignia, raised on the flap.
mesh('Courier insignia',[(0,-.304,1.31),(-.047,-.304,1.355),(-.021,-.304,1.35),(0,-.304,1.327),(.036,-.304,1.364),(.047,-.304,1.356)],[(0,1,2,3),(3,4,5,0)],7,'spine_03')
# Compact cyan neck textile with bounded tails, anchored to upper chest.
ribbon('Cyan neck textile',[(-.028,-.12,1.485),(-.046,-.163,1.475),(-.072,-.202,1.46),(-.122,-.23,1.435),(-.175,-.235,1.414)],.05,2,'spine_03',normal=(1,0,0))

# Licensed authored hairstyle. Align actual bounds to the source scalp.
before=set(bpy.context.scene.objects)
hairpath=os.path.join(ROOT,'.validation/ProductionUpgrade/BaseSources/Universal Base Characters[Standard]/Hairstyles/Origin at 0/FBX (Unity)/Hair_SimpleParted.fbx')
bpy.ops.import_scene.fbx(filepath=hairpath)
for o in set(bpy.context.scene.objects)-before:
    if o.type!='MESH':bpy.data.objects.remove(o,do_unlink=True);continue
    o.name='Swept courier hair'
    o.parent=None
    bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.transform_apply(location=False,rotation=True,scale=True);o.select_set(False)
    corners=[o.matrix_world @ Vector(c) for c in o.bound_box]
    lo=Vector([min(p[i] for p in corners) for i in range(3)]);hi=Vector([max(p[i] for p in corners) for i in range(3)])
    center=(lo+hi)/2
    scale=.245/(hi.x-lo.x)
    for v in o.data.vertices:v.co=(v.co-center)*scale+Vector((0,-.013,1.773))
    o.location=(0,0,0);o.data.materials.clear();o.data.materials.append(hairmat)
    for p in o.data.polygons:p.material_index=0;p.use_smooth=True
    o.vertex_groups.clear();skin_object(o,'Head')

# Atlas contains actual weave and restrained wear; UV regions encode garment identity.
colors=[(1,.58,.085),(.075,.115,.15),(.08,.58,.66),(.24,.145,.09),(.035,.045,.053),(.42,.45,.43),(.20,.24,.23),(.85,.86,.76)]
size=1024;pixels=array('f')
for y in range(size):
    for x in range(size):
        slot=(y//256)*4+x//256;c=colors[slot%8]
        weave=1+.021*math.sin(x*math.pi)+.014*math.sin(y*math.pi*.5)+.008*math.sin(x*.37+y*.21)
        for v in c:pixels.append(max(0,min(1,v*weave)))
        pixels.append(1)
image=bpy.data.images.new('Courier textiles atlas',width=size,height=size)
image.pixels.foreach_set(pixels);image.filepath_raw=os.path.join(OUT,'CourierTextiles.png');image.file_format='PNG';image.save()
# Unity gets four shared materials, preserving real skin/hair/eye texture UVs.
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
for o in objects:
    for m in list(o.modifiers):
        if m.type!='ARMATURE':o.modifiers.remove(m)
    # Limit influences to four and normalize.
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    bpy.ops.object.vertex_group_limit_total(limit=4);bpy.ops.object.vertex_group_normalize_all(lock_active=False)
    o.select_set(False)
rig.name='CourierRig'; rig.data.name='CourierSkeleton'
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=body
bpy.ops.object.join()
body.name='Courier skinned outfit'; objects=[body]
rig.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'ShiftboundCourier.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_ALL',mesh_smooth_type='FACE',use_mesh_modifiers=True)
os.makedirs(os.path.join(ROOT,'ArtDirection/ProductionUpgrade/Sources'),exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtDirection/ProductionUpgrade/Sources/ShiftboundCourier.blend'))
stats={'meshObjects':len(objects),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects),'vertices':sum(len(o.data.vertices) for o in objects),'materials':[m.name for m in materials],'base':'Quaternius Universal Base Characters Standard CC0','tool':bpy.app.version_string}
with open(os.path.join(ROOT,'.validation/ProductionUpgrade/courier-authored.json'),'w') as f:json.dump(stats,f,indent=2)
print('PRODUCTION COURIER AUTHORED',stats)
