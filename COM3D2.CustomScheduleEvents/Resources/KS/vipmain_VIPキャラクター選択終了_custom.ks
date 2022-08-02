*VIPキャラクター選択終了_custom
	@call file="reset_scene"
	
	@if exp= "GetManFlag('時間帯') == 2"
		@eval exp="tf['VIP時間帯'] = '昼仕事ID'"
	@elsif exp= "GetManFlag('時間帯') == 3"
		@eval exp="tf['VIP時間帯'] = '夜仕事ID'"
	@endif

	@if exp= "IsMaidTaskCustom(0,tf['VIP時間帯'], 'VIP')"
		@R_call file="VIP_main_0001_custom" label="*top"
	@else
		@jump label=*VIPキャラクター選択終了
	@endif
	@s
	
*RET_VIP_main_0001_custom
	@jump label=*VIPループ
	@s
