import bpy, json, os
from mathutils import Vector
root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=os.path.join(root, 'UnityProject/Assets/Shiftbound/ThirdParty/Quaternius/Character/Superhero_Male_FullBody.fbx'))
report=[]
for o in bpy.context.scene.objects:
    if o.type=='MESH':
        points=[o.matrix_world @ v.co for v in o.data.vertices]
        report.append(dict(name=o.name, vertices=len(points), materials=[m.name for m in o.data.materials],
            bounds=[[min(p[i] for p in points),max(p[i] for p in points)] for i in range(3)],
            groups=[g.name for g in o.vertex_groups]))
    if o.type=='ARMATURE':
        report.append(dict(name=o.name, bones=[dict(name=b.name, head=list(o.matrix_world @ b.head_local), tail=list(o.matrix_world @ b.tail_local)) for b in o.data.bones]))
with open(os.path.join(root, '.validation/ProductionUpgrade/courier-source.json'),'w') as f: json.dump(report,f,indent=2)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root, '.validation/ProductionUpgrade/courier-base.blend'))
print('COURIER SOURCE INSPECTED')
