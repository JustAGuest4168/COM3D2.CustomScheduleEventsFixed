*VIP�L�����N�^�[�I���I��_custom
	@call file="reset_scene"
	
	@if exp= "GetManFlag('���ԑ�') == 2"
		@eval exp="tf['VIP���ԑ�'] = '���d��ID'"
	@elsif exp= "GetManFlag('���ԑ�') == 3"
		@eval exp="tf['VIP���ԑ�'] = '��d��ID'"
	@endif

	@if exp= "IsMaidTaskCustom(0,tf['VIP���ԑ�'], 'VIP')"
		@R_call file="VIP_main_0001_custom" label="*top"
	@else
		@jump label=*VIP�L�����N�^�[�I���I��
	@endif
	@s
	
*RET_VIP_main_0001_custom
	@jump label=*VIP���[�v
	@s
