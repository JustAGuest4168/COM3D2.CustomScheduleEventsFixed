*top
;===========================================
	@if exp= "GetManFlag('ÔÑ') == 2"
		@eval exp="tf['VIPÔÑ'] = 'dID'"
	@elsif exp= "GetManFlag('ÔÑ') == 3"
		@eval exp="tf['VIPÔÑ'] = 'édID'"
	@endif

	@eval exp="ExecMaidTaskCustom(0,tf['VIPÔÑ'], 'VIP')"
	@R_return
	@s