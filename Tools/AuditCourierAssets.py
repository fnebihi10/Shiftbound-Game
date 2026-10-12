"""Read-only FBX/rig assessment and studio reference renders; Blender 4.5 LTS."""
import bpy, os, json, math
from mathutils import Vector
ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
OUT=os.path.join(ROOT,'.validation','CourierMilestone','AssetAudit')
os.makedirs(OUT,exist_ok=True)
reports=[]
for name in ['Casual_Humanoid','Casual_Hoodie','Superhero_Male_FullBody']:
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=os.path.join(ROOT,'UnityProject/Assets/Shiftbound/ThirdParty/Quaternius/Character',name+'.fbx'))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    rigs=[o for o in bpy.context.scene.objects if o.type=='ARMATURE']
    points=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
    lo=Vector([min(p[i] for p in points) for i in range(3)]); hi=Vector([max(p[i] for p in points) for i in range(3)])
    report=dict(asset=name,bounds=[list(lo),list(hi)],height=hi.z-lo.z,meshes=[],rigs=[])
    for o in meshes:
        report['meshes'].append(dict(name=o.name,vertices=len(o.data.vertices),triangles=sum(len(p.vertices)-2 for p in o.data.polygons),
            materials=[m.name for m in o.data.materials if m],max_influences=max((len(v.groups) for v in o.data.vertices),default=0),
            unweighted=sum(1 for v in o.data.vertices if not v.groups),groups=[g.name for g in o.vertex_groups]))
    for rig in rigs:
        report['rigs'].append(dict(name=rig.name,bones=[dict(name=b.name,parent=b.parent.name if b.parent else None,head=list(rig.matrix_world@b.head_local)) for b in rig.data.bones]))
    reports.append(report)
    # Neutral controlled lighting shows actual silhouette and garment geometry.
    bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,lo.z-.002))
    floor=bpy.context.object
    mat=bpy.data.materials.new('Neutral audit floor');mat.diffuse_color=(.16,.18,.20,1);floor.data.materials.append(mat)
    bpy.ops.object.light_add(type='AREA',location=(3,-4,5));bpy.context.object.data.energy=650;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=4
    bpy.ops.object.light_add(type='AREA',location=(-3,1,3));bpy.context.object.data.energy=400;bpy.context.object.data.size=3
    bpy.ops.object.camera_add(location=(3,-5,2.5));camera=bpy.context.object
    target=Vector((0,0,(lo.z+hi.z)*.5));camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.type='ORTHO';camera.data.ortho_scale=2.6
    scene=bpy.context.scene;scene.camera=camera;scene.render.engine='BLENDER_EEVEE_NEXT'
    scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100
    scene.world.color=(.15,.15,.15);scene.view_settings.view_transform='Standard'
    scene.render.filepath=os.path.join(OUT,name+'.png');bpy.ops.render.render(write_still=True)
with open(os.path.join(OUT,'assets.json'),'w') as f:json.dump(reports,f,indent=2)
print('COURIER ASSET AUDIT COMPLETE')
