*�鉾�n�[�����y�A�I���I��
	@eval exp="BackupReStoreCharaList()"
	;// �鉾�̃X�e�[�W�I��
	@if exp= "IsMaidTaskCustom(0,tf['�V�鉾���ԑ�'],'NewYotogi')"
		@SceneCall name=SceneYotogi start=Init label=*init_end_custom
	@else
		@SceneCall name=SceneYotogi start=Init label=*init_end
	@endif
	@s