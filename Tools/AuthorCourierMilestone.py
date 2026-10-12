"""Connected garments and equipment on the licensed Quaternius humanoid.
Writes only the isolated courier authoring project; Blender 4.5 LTS.
"""
import bpy,bmesh,os,math,json
from mathutils import Vector, Matrix
from mathutils.kdtree import KDTree
from mathutils.bvhtree import BVHTree
ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'..'))
OUT=os.path.join(ROOT,'.validation/CourierMilestone/Work/UnityProject/Assets/Shiftbound/CourierMilestone')
os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=os.path.join(ROOT,'UnityProject/Assets/Shiftbound/ThirdParty/Quaternius/Character/Superhero_Male_FullBody.fbx'))
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
body=bpy.data.objects['SuperHero_Male']
original_meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
# Author in actual metres. The source's centimetre armature transform must be
# baked before offsets/garment shaping; applying mesh local scale alone is wrong.
for o in original_meshes:
    world=o.matrix_world.copy();o.parent=None;o.matrix_world=world
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);o.select_set(False)
bpy.context.view_layer.objects.active=rig;rig.select_set(True)
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);rig.select_set(False)
for o in original_meshes:o.parent=rig;o.matrix_parent_inverse=rig.matrix_world.inverted()
bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
pack_bone=rig.data.edit_bones.new('courier_pack');pack_bone.head=(0,-.21,1.25);pack_bone.tail=(0,-.21,1.40)
pack_bone.parent=rig.data.edit_bones['spine_03'];bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
def material(name,color):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=.72
    return m
skin=material('Courier skin',(.58,.34,.22));gold=material('Courier golden canvas',(.95,.48,.06))
navy=material('Courier navy twill',(.045,.085,.12));petrol=material('Courier petrol canvas',(.035,.23,.25))
charcoal=material('Courier charcoal rubber',(.035,.045,.05));piping=material('Courier reflective seam',(.76,.78,.62))
hairmat=material('Courier cropped hair',(.085,.035,.018));eyes=material('Courier eyes',(.18,.07,.025))
for o in original_meshes:
    o.data.materials.clear();o.data.materials.append(eyes if o.name=='Eyes' else hairmat if o.name=='Eyebrows' else skin)
    for p in o.data.polygons:p.material_index=0;p.use_smooth=True
points=[v.co.copy() for v in body.data.vertices]
tree=KDTree(len(points))
for i,p in enumerate(points):tree.insert(p,i)
tree.balance()
source_weights=[[(body.vertex_groups[g.group].name,g.weight) for g in v.groups] for v in body.data.vertices]
def bind(o,rigid=None):
    o.parent=rig;o.matrix_parent_inverse=rig.matrix_world.inverted()
    if not any(m.type=='ARMATURE' for m in o.modifiers):o.modifiers.new('Courier skeleton','ARMATURE').object=rig
    if rigid:
        g=o.vertex_groups.get(rigid) or o.vertex_groups.new(name=rigid)
        for v in o.data.vertices:g.add([v.index],1,'REPLACE')
    else:
        for name in body.vertex_groups.keys():
            if not o.vertex_groups.get(name):o.vertex_groups.new(name=name)
        for v in o.data.vertices:
            _,index,_=tree.find(v.co)
            for name,w in source_weights[index]:o.vertex_groups[name].add([v.index],w,'REPLACE')
def physical_uv(o):
    uv=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
    for p in o.data.polygons:
        n=p.normal
        for li in p.loop_indices:
            v=o.data.vertices[o.data.loops[li].vertex_index].co
            uv.data[li].uv=(v.x*3,v.z*3) if abs(n.y)>.55 else (v.y*3,v.z*3) if abs(n.x)>.55 else (v.x*3,v.y*3)
def garment(name,predicate,offset,mat,smoothing):
    o=body.copy();o.data=body.data.copy();bpy.context.collection.objects.link(o);o.name=name
    bm=bmesh.new();bm.from_mesh(o.data)
    bmesh.ops.delete(bm,geom=[f for f in bm.faces if not predicate(f.calc_center_median())],context='FACES')
    for _ in range(smoothing):bmesh.ops.smooth_vert(bm,verts=list(bm.verts),factor=.22,use_axis_x=True,use_axis_y=True,use_axis_z=True)
    bm.normal_update()
    for v in bm.verts:
        amount=offset
        if name=='Connected windbreaker':
            # Ease at the waist rather than retaining sculpted abdominal muscles.
            amount+=.013*math.exp(-((v.co.z-1.10)/.19)**2)
        if name=='Work trousers':amount+=.006*math.exp(-((v.co.z-.62)/.17)**2)
        v.co+=v.normal*amount
        if name=='Connected windbreaker' and abs(v.co.x)<.18 and v.co.z>1.50:v.co.z=1.50
    if name=='Connected windbreaker':
        # Treat complete connected boundary loops, not a coordinate subset of
        # their vertices. Partial projection produced the rejected torn neckline.
        edges={e for e in bm.edges if e.is_boundary};loops=[]
        while edges:
            todo=[edges.pop()];group=set(todo)
            while todo:
                edge=todo.pop()
                for v in edge.verts:
                    for neighbour in v.link_edges:
                        if neighbour in edges:edges.remove(neighbour);group.add(neighbour);todo.append(neighbour)
            loops.append(group)
        for loop in loops:
            vertices={v for e in loop for v in e.verts};center=sum((v.co for v in vertices),Vector())/len(vertices)
            print('JACKET BOUNDARY',len(vertices),tuple(center))
            neck=center.z>1.30 and abs(center.x)<.25
            if neck:
                for v in vertices:
                    direction=Vector((v.co.x,v.co.y+.035,0)).normalized()
                    v.co=Vector((direction.x*.110,direction.y*.090-.035,1.495))
                result=bmesh.ops.extrude_edge_only(bm,edges=list(loop))
                for v in [e for e in result['geom'] if isinstance(e,bmesh.types.BMVert)]:
                    direction=Vector((v.co.x,v.co.y+.035,0)).normalized()
                    v.co=Vector((direction.x*.087,direction.y*.071-.035,1.554))
            elif abs(center.x)>.5:
                cy,cz=center.y,center.z
                for v in vertices:
                    radial=Vector((0,v.co.y-cy,v.co.z-cz)).normalized()
                    v.co=Vector((math.copysign(.705,center.x),cy+radial.y*.048,cz+radial.z*.048))
            elif center.z<1.2:
                for v in vertices:v.co.z=1.025
        bm.normal_update()
    bm.to_mesh(o.data);bm.free()
    o.data.materials.clear();o.data.materials.append(mat)
    for p in o.data.polygons:p.material_index=0;p.use_smooth=True
    physical_uv(o)
    # Thin closed boundary rims; continuous source shoulder/elbow topology kept.
    solid=o.modifiers.new('Sewn boundary thickness','SOLIDIFY');solid.thickness=.0025;solid.offset=-1
    solid.use_rim_only=True
    bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=solid.name)
    return o
jacket=garment('Connected windbreaker',lambda p:1.02<p.z<1.565 and abs(p.x)<.705 and not(abs(p.x)<.095 and p.z>1.485),.019,gold,14)
pants=garment('Work trousers',lambda p:.17<p.z<1.055,.022,navy,6)
# Covered anatomy is removed after garment copies retain their source weights.
bm=bmesh.new();bm.from_mesh(body.data)
# Cut the retained head cleanly. Face-centre deletion alone left triangular
# trapezius fragments protruding through the newly fitted neckline.
bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),plane_co=(0,0,1.58),plane_no=(0,0,1),dist=.00001)
bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.calc_center_median().z<1.57999 and abs(f.calc_center_median().x)<.705],context='FACES')
neck_edges=[e for e in bm.edges if e.is_boundary and all(abs(v.co.z-1.58)<.001 and abs(v.co.x)<.2 for v in e.verts)]
result=bmesh.ops.extrude_edge_only(bm,edges=neck_edges)
for v in [e for e in result['geom'] if isinstance(e,bmesh.types.BMVert)]:
    direction=Vector((v.co.x,v.co.y+.035,0)).normalized()
    v.co=Vector((direction.x*.063,direction.y*.048-.035,1.499))
bm.normal_update()
for f in bm.faces:f.smooth=True
bm.to_mesh(body.data);bm.free()
def mesh(name,verts,faces,mat,rigid=None):
    data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update()
    o=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(o);data.materials.append(mat)
    for p in data.polygons:p.use_smooth=True
    bind(o,rigid);physical_uv(o);return o
def loft(name,rings,mat,rigid=None,n=24,cx=0,closed=False):
    verts=[];faces=[]
    for ring in rings:
        z,rx,ry,cy=ring[:4];rowcx=ring[4] if len(ring)>4 else cx
        for i in range(n):
            a=2*math.pi*i/n;verts.append((rowcx+math.cos(a)*rx,cy+math.sin(a)*ry,z))
    for row in range(len(rings)-1):
        for i in range(n):j=(i+1)%n;faces.append((row*n+i,row*n+j,(row+1)*n+j,(row+1)*n+i))
    if closed:faces.extend([tuple(reversed(range(n))),tuple((len(rings)-1)*n+i for i in range(n))])
    return mesh(name,verts,faces,mat,rigid)
loft('Bound hem',[(1.016,.181,.137,-.035),(1.035,.184,.139,-.035),(1.05,.185,.140,-.035)],navy)
cloth_bm=bmesh.new();cloth_bm.from_mesh(jacket.data);cloth_surface=BVHTree.FromBMesh(cloth_bm);cloth_bm.free()
def ribbon(name,points,width,mat,rigid=None,fit=False):
    controls=[Vector(p) for p in points];pts=[];verts=[]
    for a,b in zip(controls,controls[1:]):
        segments=max(1,math.ceil((b-a).length/.012))
        pts.extend(a.lerp(b,i/segments) for i in range(segments))
    pts.append(controls[-1])
    normals=[];directions=[]
    if fit:
        for i,p in enumerate(pts):
            if name=='Waterproof zipper' or p.y>.035:
                origin=Vector((p.x,.5,p.z));direction=Vector((0,-1,0))
            elif p.y<-.055:
                origin=Vector((p.x,-.5,p.z));direction=Vector((0,1,0))
            else:
                origin=Vector((p.x,p.y,1.9));direction=Vector((0,0,-1))
            hit,normal,_,distance=cloth_surface.ray_cast(origin,direction,.9)
            if hit is None:raise RuntimeError('Webbing misses garment: '+name+' '+str(tuple(p)))
            if normal.dot(hit-Vector((0,-.035,1.33)))<0:normal=-normal
            pts[i]=hit+normal*.005;normals.append(normal);directions.append(direction)
    previous_side=None
    for i,p in enumerate(pts):
        tangent=pts[min(i+1,len(pts)-1)]-pts[max(0,i-1)]
        normal=normals[i] if fit else Vector((0,1,0))
        side=tangent.cross(normal)
        if side.length<.00001:side=previous_side.copy() if previous_side is not None else Vector((1,0,0))
        side.normalize()
        if previous_side is not None and side.dot(previous_side)<0:side=-side
        previous_side=side.copy();side*=width*.5
        for q in [p-side,p+side]:
            if fit:
                direction=directions[i]
                origin=Vector((q.x,.5 if direction.y<0 else -.5,q.z)) if abs(direction.y)>.5 else Vector((q.x,q.y,1.9))
                hit,edge_normal,_,distance=cloth_surface.ray_cast(origin,direction,.9)
                if hit is not None:
                    if edge_normal.dot(hit-Vector((0,-.035,1.33)))<0:edge_normal=-edge_normal
                    q=hit+edge_normal*.005
            # Directional projection preserves the selected outside surface;
            # nearest-surface projection could jump across neckline triangles.
            verts.append(q)
    o=mesh(name,verts,[(2*i,2*i+1,2*i+3,2*i+2) for i in range(len(pts)-1)],mat,rigid)
    if fit:
        solid=o.modifiers.new('Webbing thickness','SOLIDIFY');solid.thickness=.0015;solid.offset=0
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=solid.name)
    return o
# Surface fitted zipper and functional load straps. Every sampled vertex is
# attached to the finished garment surface before weights are transferred.
ribbon('Waterproof zipper',[(0,.12,1.065+i*.032) for i in range(13)]+[(0,.050,1.505),(0,.027,1.548)],.008,charcoal,fit=True)
for s in [-1,1]:
    ribbon('Load strap',[(s*.12,.12,1.12),(s*.125,.13,1.24),(s*.13,.11,1.37),(s*.14,.065,1.47),(s*.15,.00,1.51),(s*.145,-.08,1.48),(s*.12,-.16,1.39)],.025,navy,fit=True)
for s,side in [(-1,'l'),(1,'r')]:
    # Measured source sole: y [-.142,.128], width .110 m. Provide a fitted
    # 29.5 cm shell; the ankle centre differs from the forefoot centre.
    loft('Trainer sole '+side,[(.004,.064,.1475,-.0065),(.018,.065,.1475,-.0065),(.034,.064,.145,-.0065)],piping,'foot_'+side,n=24,cx=s*.131,closed=True)
    loft('Trainer upper '+side,[(.031,.062,.144,-.007,s*.131),(.062,.061,.140,-.008,s*.130),(.10,.054,.112,-.026,s*.124),(.145,.045,.069,-.060,s*.1143),(.175,.042,.051,-.075,s*.1143)],charcoal,'foot_'+side,n=24,cx=s*.131,closed=True)
loft('Sewn courier pack',[(1.08,.12,.046,-.20),(1.12,.143,.066,-.21),(1.28,.151,.072,-.215),(1.39,.14,.065,-.205),(1.43,.12,.046,-.20)],petrol,'courier_pack',closed=True)
loft('Rolled pack lip',[(1.405,.127,.047,-.205),(1.424,.13,.051,-.205),(1.445,.125,.045,-.205)],navy,'courier_pack',closed=True)
for s in [-1,1]:
    ribbon('Pack retention',[(s*.082,-.262,1.42),(s*.085,-.29,1.29),(s*.08,-.278,1.14)],.018,charcoal,'courier_pack')
mesh('Courier wing badge',[(-.052,-.29,1.32),(-.018,-.293,1.29),(0,-.296,1.315),(.042,-.294,1.35),(.053,-.29,1.338),(0,-.296,1.285)],[(0,1,2),(2,3,4,5)],piping,'courier_pack')
# Close cropped silhouette follows the licensed head topology exactly. The
# separate hairstyle's quiff floated above this differently proportioned scalp.
hair=garment('Close cropped scalp',lambda p:p.z>(1.735 if p.y>.04 else 1.70),.005,hairmat,1)
hair.vertex_groups.clear();bind(hair,'Head')
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
for o in objects:
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    bpy.ops.object.vertex_group_limit_total(limit=4);bpy.ops.object.vertex_group_normalize_all(lock_active=False)
    o.select_set(False)
rig.name='CourierRig';rig.data.name='CourierSkeleton'
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=body;bpy.ops.object.join();body.name='Courier tailored outfit'
rig.select_set(True);bpy.context.view_layer.objects.active=rig
# Convert the complete authored asset to Blender -Y forward before exporting.
# Transform rest skeleton and mesh together; never compensate with a gameplay
# visual rotation that can disagree with the Humanoid avatar.
turn=Matrix.Rotation(math.pi,4,'Z');body.data.transform(turn);rig.data.transform(turn)
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'CourierV2.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_ALL',mesh_smooth_type='FACE')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'.validation/CourierMilestone/CourierV2.blend'))
stats=dict(vertices=len(body.data.vertices),triangles=sum(len(p.vertices)-2 for p in body.data.polygons),bones=len(rig.data.bones),maxWeights=max(len(v.groups) for v in body.data.vertices),unweighted=sum(not v.groups for v in body.data.vertices))
with open(os.path.join(ROOT,'.validation/CourierMilestone/AssetAudit/authored-v2.json'),'w') as f:json.dump(stats,f,indent=2)
print('COURIER V2 AUTHORING COMPLETE',stats)
