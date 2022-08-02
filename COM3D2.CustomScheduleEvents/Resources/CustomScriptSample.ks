;
;SETUP
;
@fade out black time=0 sync
@eval exp="tf['メイド番号'] = 0"
@call file="function" label="*カスタム固定解除"

;
;EVENT ACTUALLY STARTING
;
@SetBg file=ShinShitsumu
@PlayBgm file=BGM020 fade=1000


@fade out black time=200 sync
@Camera cx=100 cy=100 cz=100 radius=0.1 rx=0 ry=0
@fade in black time=200 sync

@talk name=[HF]
OK Master, open your eyes.
@hitret

@CameraControl true
@AllPos x=0 y=0.0102 z=-1.09 rx=0 ry=180 rz=0 blend=0
@Camera cx=0.005485881 cy=1.284153 cz=-1.141864 radius=2.2 rx=-3240.183 ry=7.826151
@LightMain rx=23.2499 ry=359.4718 rz=189.58 intensity=0.95
@fade in black time=500 sync
@EyeToCamera maid=0 move=顔を向ける

@MotionAutoTwist maid=0 kata=on

@HairPhisics maid=0 hairr=nofront

@ItemSet temp maid=0 category=handitem item=HandItemR_BirthdayCake_I_.menu temp
@AllProcPropSeqStart maid=0 sync

@BustMove	maid=0	move=non
@IKDetach	src=maid:0	SrcBone="左手"
@IKDetach	src=maid:0	SrcBone="右手"

@talk name=[HF]
HAPPY BIRTHDAY MASTER!
@Motion maid=0 mot=cake_maenidasu_f_ONCE_ loop=false;
@hitret

@talk name=※Guest4168※
Thanks for using CustomScheduleEvents. This is all I can offer as a sample.
@hitret
@talk name=※Guest4168※
Check out existing KISS scripts to learn more. If you need help with any positioning and animations, saving scenes in Studio Mode and looking at the XML file may help.
@hitret
@talk name=※Guest4168※
Have fun editing VIP Events, share them with the community, and remember: Don't abuse your meidos!
@hitret

;
;END
;
@fade out black time=1000 sync
@eval exp="tf['メイド番号'] = 0"
@call file="function" label="*カスタム固定解除"

@fade3d in black time=500

@eval exp="tf['痕メイド'] = 0"
@call file=seieki label=*痕削除

@MaidReset maid=0

@R_return