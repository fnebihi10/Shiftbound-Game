import bpy, os, math
from mathutils import Vector
ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'..'))
OUT=os.path.join(ROOT,'.validation/CourierMilestone/AssetAudit')
bpy.ops.wm.open_mainfile(filepath=os.environ.get('COURIER_REVIEW_BLEND',os.path.join(ROOT,'ArtDirection/ProductionUpgrade/Sources/ShiftboundCourier.blend')))
atlas=bpy.data.images.load(os.path.join(ROOT,'.validation/ProductionUpgrade/BeforeWindowsPolishedRestore/UnityProject/Assets/Shiftbound/ProductionUpgrade/Courier/CourierTextiles.png'),check_existing=True)
for material in bpy.data.materials:
    material.use_nodes=True
    bsdf=material.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value=material.diffuse_color
    bsdf.inputs['Roughness'].default_value=.72
    if material.name=='CourierTextiles':
        image=material.node_tree.nodes.new('ShaderNodeTexImage');image.image=atlas
        material.node_tree.links.new(image.outputs['Color'],bsdf.inputs['Base Color'])
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.001))
mat=bpy.data.materials.new('Studio floor');mat.diffuse_color=(.16,.18,.20,1);bpy.context.object.data.materials.append(mat)
for position,power in [((3,4,5),600),((-3,-2,3),400)]:
    bpy.ops.object.light_add(type='AREA',location=position);bpy.context.object.data.energy=power;bpy.context.object.data.size=3
bpy.ops.object.camera_add();camera=bpy.context.object;camera.data.type='ORTHO';camera.data.ortho_scale=2.3
scene=bpy.context.scene;scene.camera=camera;scene.render.engine='BLENDER_EEVEE_NEXT'
scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100
scene.world.color=(.15,.15,.15);scene.view_settings.view_transform='Standard'
for label,position in [('front',(2.5,-5,2.5)),('rear',(-2.5,5,2.5))]:
    camera.location=position;camera.rotation_euler=(Vector((0,0,.95))-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=os.path.join(OUT,os.environ.get('COURIER_REVIEW_PREFIX','authored')+'-'+label+'.png');bpy.ops.render.render(write_still=True)
print('AUTHORED COURIER REFERENCES COMPLETE')
