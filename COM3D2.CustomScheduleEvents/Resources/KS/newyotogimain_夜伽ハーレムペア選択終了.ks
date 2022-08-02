*夜伽ハーレムペア選択終了
	@eval exp="BackupReStoreCharaList()"
	;// 夜伽のステージ選択
	@if exp= "IsMaidTaskCustom(0,tf['新夜伽時間帯'],'NewYotogi')"
		@SceneCall name=SceneYotogi start=Init label=*init_end_custom
	@else
		@SceneCall name=SceneYotogi start=Init label=*init_end
	@endif
	@s