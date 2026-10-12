"""Bake foot trajectories, balance and complete jump/recovery actions to FK.
The motor remains authoritative; the exported clips contain no travel root motion.
"""
import bpy,os,math,json
from mathutils import Vector,Quaternion,Matrix
ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'..'))
OUT=os.path.join(ROOT,'.validation/CourierMilestone/Work/UnityProject/Assets/Shiftbound/CourierMilestone')
bpy.ops.wm.open_mainfile(filepath=os.path.join(ROOT,'.validation/CourierMilestone/CourierV2.blend'))
rig=bpy.data.objects['CourierRig'];bpy.context.view_layer.objects.active=rig
rest={b.name:b.matrix_local.copy() for b in rig.data.bones}
X=Vector((1,0,0));Y=Vector((0,-1,0));Z=Vector((0,0,1))
def rotate(name,axis,angle):
    q=rest[name].to_quaternion();bone=rig.pose.bones[name]
    bone.rotation_quaternion=(q.inverted()@Quaternion(axis,angle)@q)@bone.rotation_quaternion
def aim(name,head,target):
    bone=rig.data.bones[name]
    delta=(bone.tail_local-bone.head_local).rotation_difference(target-head)
    m=delta.to_matrix().to_4x4()@rest[name];m.translation=head
    rig.pose.bones[name].matrix=m
    bpy.context.view_layer.update()
def leg(side,ankle,pitch=0,toe_roll=0):
    upper=rig.pose.bones['thigh_'+side];lower=rig.pose.bones['calf_'+side]
    hip=upper.head.copy();a=rig.data.bones['thigh_'+side].length;b=rig.data.bones['calf_'+side].length
    vector=ankle-hip;distance=min(vector.length,a+b-.002);direction=vector.normalized()
    # A target outside reach must not translate the ankle away from the calf.
    # Bake rotations on a connected chain, not hidden stretch translations.
    ankle=hip+direction*distance
    knee_forward=(Y-direction*direction.dot(Y)).normalized()
    along=(a*a+distance*distance-b*b)/(2*distance)
    knee=hip+direction*along+knee_forward*math.sqrt(max(0,a*a-along*along))
    aim('thigh_'+side,hip,knee);aim('calf_'+side,knee,ankle)
    m=Quaternion(X,pitch).to_matrix().to_4x4()@rest['foot_'+side];m.translation=ankle
    rig.pose.bones['foot_'+side].matrix=m
    # A deforming forefoot remains flat through heel lift, then relaxes in
    # swing. The Humanoid toes channel retargets to the new weighted trainers.
    rotate('ball_'+side,X,toe_roll)
    bpy.context.view_layer.update()
def arms(phase,energy=0,spread=0):
    for sign,side,offset in [(-1,'l',math.pi),(1,'r',0)]:
        upper=rig.pose.bones['upperarm_'+side];l=rig.data.bones[upper.name].length
        theta=math.cos(phase*2*math.pi+offset)*energy
        lateral=1 if upper.head.x>0 else -1
        elbow=upper.head+Vector((lateral*(.025+spread),-math.sin(theta)*l,-math.cos(theta)*l)).normalized()*l
        aim(upper.name,upper.head.copy(),elbow)
        lower=rig.pose.bones['lowerarm_'+side];length=rig.data.bones[lower.name].length
        bend=.30+min(1.2,energy*1.8)
        vector=Vector((lateral*.01,-math.sin(theta+bend)*length,-math.cos(theta+bend)*length)).normalized()*length
        aim(lower.name,lower.head.copy(),lower.head+vector)
def reset():
    for p in rig.pose.bones:p.rotation_mode='QUATERNION';p.matrix_basis=Matrix.Identity(4)

def jump_arms(p,moving,light):
    # Both arms drive a stationary takeoff. A running takeoff retains a small
    # opposing swing, then opens for balance rather than holding a run pose.
    keys=[(0,-.32),(.16,.85),(.43,.38),(.72,.12),(1,-.10)]
    theta=keys[-1][1]
    for (a,x),(b,y) in zip(keys,keys[1:]):
        if a<=p<=b:
            t=(p-a)/(b-a);t=t*t*(3-2*t);theta=x+(y-x)*t;break
    if light:theta*=.65
    for sign,side in [(-1,'l'),(1,'r')]:
        upper=rig.pose.bones['upperarm_'+side];length=rig.data.bones[upper.name].length
        angle=theta+(sign*.16*math.sin(math.pi*p) if moving else 0)
        lateral=1 if upper.head.x>0 else -1
        direction=Vector((lateral*(.045+.035*math.sin(math.pi*p)),-math.sin(angle)*length,-math.cos(angle)*length)).normalized()
        elbow=upper.head+direction*length;aim(upper.name,upper.head.copy(),elbow)
        lower=rig.pose.bones['lowerarm_'+side];length=rig.data.bones[lower.name].length
        bend=.50+.30*math.sin(math.pi*p)
        direction=Vector((lateral*.012,-math.sin(angle+bend)*length,-math.cos(angle+bend)*length)).normalized()
        aim(lower.name,lower.head.copy(),lower.head+direction*length)
def balance(height,lean=0,twist=0):
    pelvis=rig.pose.bones['pelvis'];pelvis.location=rest['pelvis'].to_3x3().inverted()@Vector((0,0,height))
    rotate('spine_01',X,lean*.40);rotate('spine_02',X,lean*.35);rotate('spine_03',X,lean*.25)
    rotate('spine_03',Z,twist);rotate('Head',X,-lean*.32)
    bpy.context.view_layer.update()
def step(p,nominal,cycle,stance,lift):
    stride=nominal*cycle*stance
    if p<stance:
        z=stride*(.5-p/stance);height=0
        pitch=math.radians(8)*(1-p/(stance*.14)) if p<stance*.14 else -math.radians(18)*max(0,(p/stance-.75)/.25)
        toe_roll=max(0,-pitch)
    else:
        t=(p-stance)/(1-stance)
        z=-stride*.5+stride*(t*t*(3-2*t));height=lift*math.sin(math.pi*t)**1.15
        # Preserve the push-off angle at lift, rather than snapping from -18
        # degrees to zero. Ease through clearance into the next heel strike.
        keys=[(0,-18),(.28,-14),(.60,-7),(.86,8),(1,8)]
        pitch=math.radians(keys[-1][1])
        for (a,x),(b,y) in zip(keys,keys[1:]):
            if a<=t<=b:
                blend=(t-a)/(b-a);blend=blend*blend*(3-2*blend)
                pitch=math.radians(x+(y-x)*blend);break
        release=min(1,t/.20);release=release*release*(3-2*release)
        toe_roll=math.radians(18)*(1-release)
    # Raise the ankle as the heel/toe rotates around its contact edge.
    raise_for_roll=max(0,math.sin(abs(pitch))*(.075 if pitch>0 else .12))
    return z,height+raise_for_roll,pitch,toe_roll
metadata=[]
specs=[('CourierIdle',2.0,None),('CourierWalk',.90,(1.5,.55,.09)),('CourierRun',.64,(3.6,.32,.22)),('CourierSprint',.45,(7.5,.23,.28)),
       ('CourierJumpStanding',1.0,None),('CourierJumpMoving',1.0,None),('CourierJumpLightStanding',1.0,None),('CourierJumpLightMoving',1.0,None),('CourierFall',.8,None),('CourierLandSoft',.42,None),('CourierLandHard',.55,None)]
for name,duration,gait in specs:
    action=bpy.data.actions.new(name);rig.animation_data_create();rig.animation_data.action=action
    frames=round(duration*60);bpy.context.scene.render.fps=60
    for frame in range(frames+1):
        bpy.context.scene.frame_set(frame);reset();p=frame/frames
        if gait:
            nominal,stance,lift=gait
            if nominal<2:
                height=-.085-.015*math.cos(p*4*math.pi)
            else:
                support_phase=p%.5
                height=-.080-.025*math.sin(math.pi*support_phase/stance)**2 if support_phase<stance else -.080+.035*math.sin(math.pi*(support_phase-stance)/(.5-stance))**2
            lean=.035 if nominal<2 else .11 if nominal<5 else .19
            balance(height,lean,.035*math.sin(p*2*math.pi))
            for side,offset in [('l',0),('r',.5)]:
                z,h,pitch,toe_roll=step((p+offset)%1,nominal,duration,stance,lift)
                ankle=rig.data.bones['foot_'+side].head_local.copy()+Y*z+Z*h
                leg(side,ankle,-pitch,-toe_roll)
            arms(p,.12 if nominal<2 else .45 if nominal<5 else .65)
        elif name.startswith('CourierJump'):
            moving=name.endswith('Moving');light='Light' in name;fold=math.sin(math.pi*p)**1.4*(.70 if light else 1)
            balance(-.035*math.exp(-((p-.025)/.07)**2),(.10+.10*fold) if moving else .035+.045*fold)
            for sign,side in [(-1,'l'),(1,'r')]:
                # Moving takeoff carries a leading knee and a trailing heel.
                # Both unfold into descent instead of holding a symmetric tuck.
                lift=(.34 if sign>0 else .23) if moving else .29
                ankle=rig.data.bones['foot_'+side].head_local.copy()+Y*(fold*(.30*sign if moving else .06))+Z*(lift*fold)
                leg(side,ankle,(.32 if moving and sign<0 else .16)*math.sin(math.pi*p))
            jump_arms(p,moving,light)
        elif name=='CourierFall':
            balance(0,.07,.012*math.sin(p*2*math.pi))
            for sign,side in [(-1,'l'),(1,'r')]:
                ankle=rig.data.bones['foot_'+side].head_local.copy()+Y*(sign*.12)+Z*(.12+.018*math.sin(p*2*math.pi+sign))
                leg(side,ankle,.13 if side=='l' else -.08)
            arms(.12+.025*math.sin(p*2*math.pi),.24,.115)
        elif name.startswith('CourierLand'):
            depth=.16 if name.endswith('Hard') else .085
            if p<.15:compress=math.sin(.5*math.pi*p/.15)
            else:
                recover=min(1,(p-.15)/.85);compress=1-recover*recover*(3-2*recover)
            balance(-depth*compress,.10*compress)
            for side in ['l','r']:leg(side,rig.data.bones['foot_'+side].head_local.copy(),0)
            arms(.08,.13+.18*compress)
        else:
            balance(.0015*math.sin(p*2*math.pi),0)
            for side in ['l','r']:leg(side,rig.data.bones['foot_'+side].head_local.copy(),0)
            arms(0,0)
        # Relaxed fingers, not the source T-pose's spread, rigid hands. Curl in
        # each hand's rest frame so arm swing carries the pose naturally.
        for side,sign in [('l',1),('r',-1)]:
            for finger in ['index','middle','ring','pinky']:
                for joint,amount in [('01',.22),('02',.35),('03',.24)]:
                    name_f=finger+'_'+joint+'_'+side
                    if name_f in rig.pose.bones:rotate(name_f,Vector((0,1,0)),sign*amount)
        for bone in rig.pose.bones:
            bone.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=bone.name)
            if bone.name=='pelvis':bone.keyframe_insert(data_path='location',frame=frame,group=bone.name)
    metadata.append(dict(name=name,duration=duration,nominalSpeed=gait[0] if gait else 0,stance=gait[1] if gait else 0))
    action.use_fake_user=True
rig.animation_data.action=None;reset();bpy.context.view_layer.update()
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'CourierMotion.fbx'),use_selection=True,object_types={'ARMATURE'},add_leaf_bones=False,
    bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_step=1,bake_anim_simplify_factor=0,
    axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_ALL')
with open(os.path.join(OUT,'MotionAuthoring.json'),'w') as f:json.dump(metadata,f,indent=2)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'.validation/CourierMilestone/CourierMotion.blend'))
print('COURIER MOTION AUTHORING COMPLETE',len(metadata),'actions')
