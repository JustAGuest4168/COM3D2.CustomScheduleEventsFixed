*init_end_custom
	@eval exp="tf['customscript'] = GetMaidTaskCustom(0,tf['�V�鉾���ԑ�'],'NewYotogi')"
	@call file=&tf['customscript']
	@eval exp="KSLog('NewYotogiMain CUSTOM SCRIPT END')"
	@MessageWindow off
	@eval exp="KSLog('NewYotogiMain MessageWindow END')"
	@jump label=*play_end
	@eval exp="KSLog('NewYotogiMain Jump Play_End END')"
	@s