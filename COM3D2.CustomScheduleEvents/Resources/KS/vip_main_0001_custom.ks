*top
;===========================================
	@if exp= "GetManFlag('���ԑ�') == 2"
		@eval exp="tf['VIP���ԑ�'] = '���d��ID'"
	@elsif exp= "GetManFlag('���ԑ�') == 3"
		@eval exp="tf['VIP���ԑ�'] = '��d��ID'"
	@endif

	@eval exp="ExecMaidTaskCustom(0,tf['VIP���ԑ�'], 'VIP')"
	@R_return
	@s