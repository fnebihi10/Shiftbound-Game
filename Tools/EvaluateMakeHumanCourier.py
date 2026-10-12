"""Isolated CC0 character evaluation; does not modify the playable project.

Uses the official MPFB Python API in Blender. Generator code stays outside the
game. Core graphics assets retain their CC0 license and provenance.
"""
import bpy, os, sys, json, math, bmesh
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from mathutils.kdtree import KDTree

ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'..'))
WORK=os.path.join(ROOT,'.validation','CourierMilestone','MPFB')
USER=os.path.join(WORK,'User')
sys.path.insert(0,os.path.join(WORK,'mpfb2-2.0.18','src'))
# The source checkout is not an installed Blender extension. Keep its extension
# data sandboxed without changing system preferences or the addon source.
original_extension_path=bpy.utils.extension_path_user
bpy.utils.extension_path_user=lambda package,*args,**kwargs: USER if package=='mpfb' else original_extension_path(package,*args,**kwargs)
bpy.context.preferences.addons.new().module='mpfb'
import mpfb
from mpfb._preferences import MpfbPreferences
bpy.utils.register_class(MpfbPreferences)
mpfb.set_preference('mpfb_user_data',USER)
mpfb.register()
mpfb.set_preference('mpfb_user_data',USER)
from mpfb.services.humanservice import HumanService
from mpfb.services.targetservice import TargetService
from mpfb.services.exportservice import ExportService
from mpfb.services.objectservice import ObjectService

bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
macro=TargetService.get_default_macro_info_dict()
macro.update(gender=1.0,age=.30,muscle=.42,weight=.42,proportions=.60,height=.5)
macro['race']={'african':.55,'asian':.20,'caucasian':.25}
human=HumanService.create_human(macro_detail_dict=macro)
human.name='Courier anatomy evaluation'
DATA=os.path.join(USER,'data')
HumanService.set_character_skin(os.path.join(DATA,'skins','young_african_male','young_african_male.mhmat'),human,skin_type='GAMEENGINE')
rig=HumanService.add_builtin_rig(human,'mixamo_unity')
for folder,name,kind in [('eyes','low-poly','Eyes'),('eyebrows','eyebrow002','Eyebrows'),('hair','short02','Hair'),('clothes','male_casualsuit05','Clothes'),('clothes','shoes05','Clothes')]:
    HumanService.add_mhclo_asset(os.path.join(DATA,folder,name,name+'.mhclo'),human,asset_type=kind,subdiv_levels=0,material_type='GAMEENGINE')

# Fit a commuter-sized parcel pack to the actual coat, not to a generic torso.
coat=next(o for o in bpy.context.scene.objects if o.type=='MESH' and 'male_casualsuit05' in o.name)
bm=bmesh.new();bm.from_mesh(coat.data);surface=BVHTree.FromBMesh(bm);bm.free()
weight_tree=KDTree(len(coat.data.vertices))
for vertex in coat.data.vertices:weight_tree.insert(vertex.co,vertex.index)
weight_tree.balance()
def cloth_point(point,side):
    p=Vector(point)
    direction=Vector((0,1,0)) if side=='front' else Vector((0,-1,0)) if side=='back' else Vector((0,0,-1))
    origin=Vector((p.x,-.6,p.z)) if side=='front' else Vector((p.x,.6,p.z)) if side=='back' else Vector((p.x,p.y,1.8))
    hit,normal,_,_=surface.ray_cast(origin,direction,1.2)
    if hit is None:raise RuntimeError('Pack strap misses coat: '+str((point,side)))
    if normal.dot(-direction)<0:normal=-normal
    return hit+normal*.005,normal
def mat(name,color):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    bsdf=m.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Base Color'].default_value=(*color,1);bsdf.inputs['Roughness'].default_value=.8
    return m
canvas=mat('Courier pack golden canvas',(.75,.34,.055))
webbing=mat('Courier pack navy webbing',(.025,.055,.075))
reflector=mat('Courier pack reflective marker',(.7,.76,.65))
# Canvas weave is authored at a modest resolution and survives close review.
# Its neutral albedo multiplies the material tint; the normal is tangent-space.
texture_dir=os.path.join(WORK,'PackTextures');os.makedirs(texture_dir,exist_ok=True)
n=512;y,x=np.mgrid[0:n,0:n];rng=np.random.default_rng(1742)
thread=(np.sin(x*math.tau/8)*np.cos(y*math.tau/8))*.023
grain=rng.normal(0,.007,(n,n));shade=np.clip(.92+thread+grain,0,1)
for name,values in [('CourierCanvas',np.stack([shade,shade,shade,np.ones_like(shade)],axis=-1)),
                    ('CourierCanvasNormal',np.stack([.5+.055*np.sin(x*math.tau/8),.5+.055*np.sin(y*math.tau/8),np.ones_like(shade),np.ones_like(shade)],axis=-1))]:
    tex=bpy.data.images.new(name,width=n,height=n,alpha=True);tex.pixels.foreach_set(values.astype(np.float32).ravel());tex.filepath_raw=os.path.join(texture_dir,name+'.png');tex.file_format='PNG';tex.save()
    if name.endswith('Normal'):tex.colorspace_settings.name='Non-Color'
    node=canvas.node_tree.nodes.new('ShaderNodeTexImage');node.image=tex
    if name.endswith('Normal'):
        normal=canvas.node_tree.nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.45
        canvas.node_tree.links.new(node.outputs['Color'],normal.inputs['Color']);canvas.node_tree.links.new(normal.outputs['Normal'],canvas.node_tree.nodes['Principled BSDF'].inputs['Normal'])
    else:
        tint=canvas.node_tree.nodes.new('ShaderNodeMixRGB');tint.blend_type='MULTIPLY';tint.inputs[0].default_value=1;tint.inputs[2].default_value=(.75,.34,.055,1)
        canvas.node_tree.links.new(node.outputs['Color'],tint.inputs[1]);canvas.node_tree.links.new(tint.outputs[0],canvas.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
back,_=cloth_point((0,0,1.20),'back')
bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
bone=rig.data.edit_bones.new('courier_pack');bone.head=(0,back.y+.01,1.24);bone.tail=(0,back.y+.01,1.31);bone.parent=rig.data.edit_bones['mixamorig:Spine2']
bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
def mesh(name,vertices,faces,material,rigid=True):
    data=bpy.data.meshes.new(name);data.from_pydata(vertices,[],faces);data.update()
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);obj.parent=rig;data.materials.append(material)
    modifier=obj.modifiers.new('Courier rig','ARMATURE');modifier.object=rig
    if rigid:
        group=obj.vertex_groups.new(name='courier_pack');group.add(list(range(len(vertices))),1,'REPLACE')
    else:
        for group in coat.vertex_groups:obj.vertex_groups.new(name=group.name)
        for vertex in obj.data.vertices:
            _,index,_=weight_tree.find(vertex.co)
            for influence in coat.data.vertices[index].groups:obj.vertex_groups[influence.group].add([vertex.index],influence.weight,'REPLACE')
    uv=data.uv_layers.new(name='UVMap')
    for face in data.polygons:
        face.use_smooth=True
        for loop in face.loop_indices:
            vertex=data.vertices[data.loops[loop].vertex_index].co;uv.data[loop].uv=(vertex.x*3,vertex.z*3)
    return obj
def shell(name,rows,material):
    vertices=[];faces=[];n=32
    for z,width,depth in rows:
        rear,_=cloth_point((0,0,z),'back');front=rear.y+.004
        for i in range(n):
            angle=math.tau*i/n;c=math.cos(angle);s=math.sin(angle)
            vertices.append((width*.5*math.copysign(abs(c)**.45,c),front+depth*.5+depth*.5*math.copysign(abs(s)**.45,s),z))
    for row in range(len(rows)-1):
        for i in range(n):j=(i+1)%n;faces.append((row*n+i,row*n+j,(row+1)*n+j,(row+1)*n+i))
    faces.extend([tuple(reversed(range(n))),tuple((len(rows)-1)*n+i for i in range(n))])
    return mesh(name,vertices,faces,material)
pack=shell('Fitted parcel pack',[(.91,.205,.065),(.925,.23,.085),(.95,.245,.10),(1.00,.25,.112),(1.05,.253,.116),(1.10,.25,.113),(1.15,.245,.105),(1.20,.23,.092),(1.235,.215,.077),(1.25,.205,.07)],canvas)
# Slight fabric tension, confined to the outward face, keeps the fitted inner
# panel flush against the coat. This is geometry, not a per-frame effect.
for vertex in pack.data.vertices:
    z=vertex.co.z;rear,_=cloth_point((0,0,z),'back');outward=max(0,min(1,(vertex.co.y-rear.y-.025)/.055))
    vertex.co.y+=outward*.0025*math.sin(z*93+vertex.co.x*19)
pack_bm=bmesh.new();pack_bm.from_mesh(pack.data);pack_surface=BVHTree.FromBMesh(pack_bm);pack_bm.free()
def pack_point(x,z):
    hit,normal,_,_=pack_surface.ray_cast(Vector((x,.7,z)),Vector((0,-1,0)),1)
    if hit is None:raise RuntimeError('Retention detail misses pack surface')
    if normal.y<0:normal=-normal
    return hit+normal*.003

def piping(name,points,radius=.0014):
    vertices=[];faces=[];sides=6
    for i,point in enumerate(points):
        p=Vector(point);tangent=(Vector(points[min(i+1,len(points)-1)])-Vector(points[max(i-1,0)])).normalized()
        u=tangent.cross(Vector((0,1,0))).normalized();v=tangent.cross(u).normalized()
        for j in range(sides):vertices.append(p+radius*(u*math.cos(j*math.tau/sides)+v*math.sin(j*math.tau/sides)))
    for i in range(len(points)-1):
        for j in range(sides):faces.append((i*sides+j,i*sides+(j+1)%sides,(i+1)*sides+(j+1)%sides,(i+1)*sides+j))
    return mesh(name,vertices,faces,webbing)

# Bound gusset seams and an actual shallow zipped utility pocket. The seams
# follow the fitted shell and stay below the readable shoulder silhouette.
for sign in [-1,1]:piping('Parcel bound side seam',[pack_point(sign*.096,.94+i*.285/36) for i in range(37)])
verts=[]
for z,width in [(.956,.135),(.962,.151),(1.04,.151),(1.064,.139)]:
    for i in range(9):
        p=pack_point(-width*.5+width*i/8,z);p.y+=.007*math.sin(math.pi*i/8);verts.append(p)
mesh('Parcel zipped utility pocket',verts,[(r*9+i,r*9+i+1,(r+1)*9+i+1,(r+1)*9+i) for r in range(3) for i in range(8)],canvas)
piping('Utility pocket zipper',[pack_point(-.067+i*.134/32,1.059)+Vector((0,.008,0)) for i in range(33)],.002)
# A compact pull belongs to the zipper rather than floating on the canvas.
p=pack_point(.058,1.059)+Vector((0,.010,0))
mesh('Utility zipper pull',[p+Vector((dx,dy,dz)) for dx,dy,dz in [(-.004,0,0),(.004,0,0),(.004,0,-.014),(-.004,0,-.014)]],[(0,1,2,3)],webbing)
shell('Rolled pack opening',[(1.22,.225,.078),(1.25,.23,.083),(1.265,.218,.073)],webbing)
for sign in [-1,1]:
    controls=[((sign*.09,0,1.0),'front'),((sign*.10,0,1.12),'front'),((sign*.12,0,1.24),'front'),((sign*.135,-.025,1.30),'top'),((sign*.135,.045,1.30),'top'),((sign*.115,0,1.245),'back'),((sign*.095,0,1.22),'back')]
    points=[]
    for (a,side),(b,next_side) in zip(controls,controls[1:]):
        a=Vector(a);b=Vector(b);steps=max(1,math.ceil((b-a).length/.009))
        for i in range(steps):
            p=a.lerp(b,i/steps);points.append((p,side if i/steps<.5 else next_side))
    points.append((Vector(controls[-1][0]),controls[-1][1]))
    centres=[cloth_point(p,side) for p,side in points];vertices=[];last=None
    for i,((p,normal),(_,side)) in enumerate(zip(centres,points)):
        tangent=centres[min(i+1,len(centres)-1)][0]-centres[max(0,i-1)][0]
        width=tangent.cross(normal).normalized()
        if last is not None and width.dot(last)<0:width=-width
        last=width
        for edge in [-1,1]:
            hit,_=cloth_point(p+width*(edge*.014),side);vertices.append(hit)
    mesh('Fitted shoulder load strap',vertices,[(2*i,2*i+1,2*i+3,2*i+2) for i in range(len(points)-1)],webbing,rigid=False)
    vertices=[]
    for i in range(31):
        z=1.225-(1.225-.945)*i/30
        vertices.extend([pack_point(sign*.08-.008,z),pack_point(sign*.08+.008,z)])
    mesh('Parcel compression strap',vertices,[(2*i,2*i+1,2*i+3,2*i+2) for i in range(30)],webbing)
vertices=[pack_point(x,z) for x,z in [(-.045,1.13),(-.015,1.10),(.042,1.145),(.047,1.13),(-.015,1.08),(-.048,1.115)]]
mesh('Courier reflective mark',vertices,[(0,1,4),(0,4,5),(1,2,3),(1,3,4)],reflector)

# The full authorable master keeps helpers; the export hierarchy bakes masks,
# removes helpers and has no subdivision cost. Nothing enters Unity yet.
master=os.path.join(WORK,'CourierAnatomyMaster.blend')
bpy.ops.wm.save_as_mainfile(filepath=master)
export_root=ExportService.create_character_copy(human,name_suffix='_evaluation')
export_body=ObjectService.find_object_of_type_amongst_nearest_relatives(export_root,'Basemesh')
ExportService.bake_modifiers_remove_helpers(export_body,bake_masks=True,bake_subdiv=False,remove_helpers=True,also_proxy=True)
export_objects=[export_root]+ObjectService.get_list_of_children(export_root)
export_rig=next(obj for obj in export_objects if obj.type=='ARMATURE')
for obj in export_objects:
    if obj.type=='MESH':
        for modifier in obj.modifiers:
            if modifier.type=='ARMATURE':modifier.object=export_rig
for obj in bpy.context.scene.objects:obj.hide_render=obj not in export_objects
stats=[]
for obj in export_objects:
    if obj.type=='MESH':
        stats.append({'name':obj.name,'vertices':len(obj.data.vertices),'triangles':sum(len(p.vertices)-2 for p in obj.data.polygons)})
        bpy.context.view_layer.objects.active=obj
        bpy.ops.object.vertex_group_limit_total(limit=4)
        bpy.ops.object.vertex_group_normalize_all(lock_active=False)
# Consolidate only the courier's parcel kit. The shared armature, three material
# regions and coat-derived strap weights survive, with one renderer instead of
# twelve tiny independently skinned details.
parcel=[obj for obj in export_objects if obj.type=='MESH' and any(obj.name.startswith(prefix) for prefix in ['Fitted ','Parcel ','Rolled ','Utility ','Courier reflective'])]
bpy.ops.object.select_all(action='DESELECT')
for obj in parcel:obj.select_set(True)
bpy.context.view_layer.objects.active=parcel[0];bpy.ops.object.join();bpy.context.object.name='Courier parcel kit'
export_objects=[export_root]+ObjectService.get_list_of_children(export_root)
bpy.ops.object.select_all(action='DESELECT')
for obj in export_objects:obj.select_set(True)
bpy.context.view_layer.objects.active=export_root
bpy.ops.export_scene.fbx(filepath=os.path.join(WORK,'CourierAnatomyEvaluation.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_ALL',path_mode='COPY',embed_textures=False)

# Studio images assess a clothed, complete silhouette. They are not gameplay
# evidence and are not called a finished courier or accepted art direction.
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.005))
floor=bpy.context.object;floor.hide_render=False
material=bpy.data.materials.new('Inspection floor');material.diffuse_color=(.13,.16,.18,1)
floor.data.materials.append(material)
for position,power in [((3,-4,5),600),((-3,2,3),400)]:
    bpy.ops.object.light_add(type='AREA',location=position);bpy.context.object.data.energy=power;bpy.context.object.data.size=3
bpy.ops.object.camera_add();camera=bpy.context.object;camera.data.type='ORTHO';camera.data.ortho_scale=2.3
scene=bpy.context.scene;scene.camera=camera;scene.render.engine='BLENDER_EEVEE_NEXT'
scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100
scene.world.color=(.15,.15,.15);scene.view_settings.view_transform='Standard'
for label,position in [('front',(2.5,-5,2.5)),('rear',(-2.5,5,2.5))]:
    camera.location=position;camera.rotation_euler=(Vector((0,0,.95))-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=os.path.join(WORK,'clothed-'+label+'.png');bpy.ops.render.render(write_still=True)
with open(os.path.join(WORK,'evaluation.json'),'w') as out:json.dump({'mpfb':'2.0.18','macro':macro,'meshes':stats,'scope':'Isolated anatomical/clothing evaluation, not integrated or accepted'},out,indent=2)
print('CLOTHED COURIER ANATOMY EVALUATION COMPLETE',stats)
