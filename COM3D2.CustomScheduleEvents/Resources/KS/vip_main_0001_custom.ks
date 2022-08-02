*top
;===========================================
	@if exp= "GetManFlag('ŠÔ‘Ñ') == 2"
		@eval exp="tf['VIPŠÔ‘Ñ'] = '’‹d–ID'"
	@elsif exp= "GetManFlag('ŠÔ‘Ñ') == 3"
		@eval exp="tf['VIPŠÔ‘Ñ'] = '–éd–ID'"
	@endif

	@eval exp="ExecMaidTaskCustom(0,tf['VIPŠÔ‘Ñ'], 'VIP')"
	@R_return
	@s