self.CustomScheduleEvents = function CustomScheduleEvents(){};

self.CustomScheduleEvents.ajaxError = function(id)
{
	if(id!= null)
	{
		if(self.GuestUI.ajaxRequests.reqs[id].hasOwnProperty("respData"))
		{
			alert(self.GuestUI.ajaxRequests.reqs[id].respData);
		}
	}
}

//Category-New
self.CustomScheduleEvents.resetNewCategoryModal = function()
{
	$("#createCategoryModal-Id").val("");
	$("#createCategoryModal-Name").val("");
};
self.CustomScheduleEvents.openNewCategoryModal = function()
{
	//Reset
	self.CustomScheduleEvents.resetNewCategoryModal();
	
	//Open
	$('#createCategoryModal').modal('show');
};
self.CustomScheduleEvents.closeNewCategoryModal = function(id)
{
	if(id!= null)
	{
		if(self.GuestUI.ajaxRequests.reqs[id].hasOwnProperty("respData"))
		{
			alert(self.GuestUI.ajaxRequests.reqs[id].respData);
		}
	}
	
	//Reset
	self.CustomScheduleEvents.resetNewCategoryModal();
	
	//Open
	$('#createCategoryModal').modal('hide');
}
self.CustomScheduleEvents.createNewCategoryParams = function()
{
	var data = {};
	data.Id = $("#createCategoryModal-Id").val();
	data.Name = $("#createCategoryModal-Name").val();
	return data;
}
self.CustomScheduleEvents.createNewCategory = function()
{
	self.GuestUI.executeAjax("scheduleCategoriesCreate");
}

//Category-Edit
self.CustomScheduleEvents.resetEditCategoryModal = function(row)
{
	if(row == null)
	{
		$("#editCategoryModal-Id").val("");
		$("#editCategoryModal-Name").val("");
	}
	else
	{
		$("#editCategoryModal-Id").val(row[0]);
		$("#editCategoryModal-Name").val(row[1]);
	}
};
self.CustomScheduleEvents.openEditCategoryModal = function(table, row)
{
	//Reset
	self.CustomScheduleEvents.resetEditCategoryModal(row);
	
	//Open
	$('#editCategoryModal').modal('show');
};
self.CustomScheduleEvents.closeEditCategoryModal = function(id)
{
	if(id!= null)
	{
		if(self.GuestUI.ajaxRequests.reqs[id].hasOwnProperty("respData"))
		{
			alert(self.GuestUI.ajaxRequests.reqs[id].respData);
		}
	}
	
	//Reset
	self.CustomScheduleEvents.resetEditCategoryModal();
	
	//Open
	$('#editCategoryModal').modal('hide');
}
self.CustomScheduleEvents.saveEditCategoryParams = function()
{
	var data = {};
	data.Id = $("#editCategoryModal-Id").val();
	data.Name = $("#editCategoryModal-Name").val();
	return data;
}
self.CustomScheduleEvents.saveEditCategory = function()
{
	self.GuestUI.executeAjax("scheduleCategoriesEdit");
}

//VIP-New
self.CustomScheduleEvents.resetNewVIPModal = function()
{
	$("#createVIPModal-Id").val("");
	$("#createVIPModal-Name").val("");
	self.GuestUI.setDropdownValue("createVIPModal-Icon", 0);
    self.GuestUI.setDropdownValue("createVIPModal-Category", 0);
    $("#createVIPModal-Description").val("");
    $("#createVIPModal-Income").val("");
    for(let i=0; i<9; i++)
	{
		$("#createVIPModal-Hint0"+(i+1)).val("");
	}
    self.GuestUI.setDropdownValue("createVIPModal-SalonGrade",0);
    
    //Normal
    //Basic
    self.GuestUI.setToggleValue("createVIPModal-ImproveRelation", false);
    self.GuestUI.setToggleValue("createVIPModal-EntertainedMaster", false);
    self.GuestUI.setToggleValue("createVIPModal-EntertainedGuest", false);
    //Requirements
    self.GuestUI.setToggleValue("createVIPModal-MainTrio", false);
    self.GuestUI.setMultiSelectValues("createVIPModal-Personality", []);
    self.GuestUI.setToggleValue("createVIPModal-Contract-Free", false);
    self.GuestUI.setToggleValue("createVIPModal-Contract-Exclusive", false);
    self.GuestUI.setToggleValue("createVIPModal-Contract-Trainee", false);
    self.GuestUI.setToggleValue("createVIPModal-HoleExp-Virgin", false);
    self.GuestUI.setToggleValue("createVIPModal-HoleExp-Vag", false);
    self.GuestUI.setToggleValue("createVIPModal-HoleExp-Anal", false);
    self.GuestUI.setToggleValue("createVIPModal-HoleExp-Both", false);
    self.GuestUI.setMultiSelectValues("createVIPModal-Relationship", []);
    self.GuestUI.setMultiSelectValues("createVIPModal-Propensity", []);
    self.GuestUI.setMultiSelectValues("createVIPModal-JobClass", []);
    self.GuestUI.setMultiSelectValues("createVIPModal-NightClass", []);
	
	//Rental
	//Basic
	self.GuestUI.setDropdownValue("createVIPModal-RentalMaid");
	
	//Enable all controls
	/*self.GuestUI.enableDropdown("createVIPModal-RentalMaid");
	
	$("#createVIPModal-ImproveRelation").prop("disabled", false);
	$("#createVIPModal-EntertainedMaster").prop("disabled", false);
	$("#createVIPModal-EntertainedGuest").prop("disabled", false);
	
	$("#createVIPModal-MainTrio").prop("disabled", false);
	$("#createVIPModal-Contract-Free").prop("disabled", false);
	$("#createVIPModal-Contract-Exclusive").prop("disabled", false);
	$("#createVIPModal-Contract-Trainee").prop("disabled", false);
	$("#createVIPModal-HoleExp-Virgin").prop("disabled", false);
	$("#createVIPModal-HoleExp-Vag").prop("disabled", false);
	$("#createVIPModal-HoleExp-Anal").prop("disabled", false);
	$("#createVIPModal-HoleExp-Both").prop("disabled", false);
	$("#createVIPModal-Personality").prop("disabled", false);
	$("#createVIPModal-Propensity").prop("disabled", false);
	$("#createVIPModal-Relationship").prop("disabled", false);
	$("#createVIPModal-JobClass").prop("disabled", false);
	$("#createVIPModal-NightClass").prop("disabled", false);*/
	
	$("#createVIPModal-basic-normal").parent().css("display", "");
	$("#createVIPModal-requirements-normal").parent().css("display", "");
	$("#createVIPModal-basic-rental").parent().css("display", "");
};
self.CustomScheduleEvents.openNewVIPModalNormal = function()
{
	//Reset
	self.CustomScheduleEvents.resetNewVIPModal();
	
	//Hide Rental controls
	//self.GuestUI.disableDropdown("createVIPModal-RentalMaid");
	$("#createVIPModal-basic-rental").parent().css("display", "none");
	
	//Open
	$('#createVIPModal').modal('show');
};
self.CustomScheduleEvents.openNewVIPModalRental = function()
{
	//Reset
	self.CustomScheduleEvents.resetNewVIPModal();
	
	//Hide Normal controls
	/*
	$("#createVIPModal-ImproveRelation").prop("disabled", true);
	$("#createVIPModal-EntertainedMaster").prop("disabled", true);
	$("#createVIPModal-EntertainedGuest").prop("disabled", true);
	
	$("#createVIPModal-MainTrio").prop("disabled", true);
	$("#createVIPModal-Contract-Free").prop("disabled", true);
	$("#createVIPModal-Contract-Exclusive").prop("disabled", true);
	$("#createVIPModal-Contract-Trainee").prop("disabled", true);
	$("#createVIPModal-HoleExp-Virgin").prop("disabled", true);
	$("#createVIPModal-HoleExp-Vag").prop("disabled", true);
	$("#createVIPModal-HoleExp-Anal").prop("disabled", true);
	$("#createVIPModal-HoleExp-Both").prop("disabled", true);
	$("#createVIPModal-Personality").prop("disabled", true);
	$("#createVIPModal-Propensity").prop("disabled", true);
	$("#createVIPModal-Relationship").prop("disabled", true);
	$("#createVIPModal-JobClass").prop("disabled", true);
	$("#createVIPModal-NightClass").prop("disabled", true);
	*/
	$("#createVIPModal-basic-normal").parent().css("display", "none");
	$("#createVIPModal-requirements-normal").parent().css("display", "none");
	
	//Open
	$('#createVIPModal').modal('show');
};
self.CustomScheduleEvents.closeNewVIPModal = function(id)
{
	if(id!= null)
	{
		if(self.GuestUI.ajaxRequests.reqs[id].hasOwnProperty("respData"))
		{
			alert(self.GuestUI.ajaxRequests.reqs[id].respData);
		}
	}
	
	//Reset
	self.CustomScheduleEvents.resetNewVIPModal();
	
	//Open
	$('#createVIPModal').modal('hide');
}
self.CustomScheduleEvents.createNewVIPParams = function()
{
	//Shared
	var data = {};
	data.rental = $("#createVIPModal-basic-normal").parent().css("display") == "none";
	
	data.Id = parseInt($("#createVIPModal-Id").val()) || 99999;
	data.Name = $("#createVIPModal-Name").val() || "";
	data.Icon = $("#createVIPModal-Icon").selectpicker('val');
	data.CategoryId = parseInt($("#createVIPModal-Category").selectpicker('val'));
	data.Description = $("#createVIPModal-Description").val() || "";
	data.Income = parseInt($("#createVIPModal-Income").val()) || 0;
	data.Hints = [];
	for(let i=0; i<9; i++)
	{
		var val = $("#createVIPModal-Hint0"+(i+1)).val();
		if( val!= null && val.trim() != "")
		{
			data.Hints.push(val);
		}
	}
	data.SalonGrade = parseInt($("#createVIPModal-SalonGrade").selectpicker('val'));
	
	//Normal
	if(!data.rental)
	{
		//Basic
		data.ImproveCustomerRelation = self.GuestUI.getToggleValue("createVIPModal-ImproveRelation");
		data.EntertainedMaster = self.GuestUI.getToggleValue("createVIPModal-EntertainedMaster");
		data.EntertainedGuest = self.GuestUI.getToggleValue("createVIPModal-EntertainedGuest");
		
		//Requirements
		data.MainTrio = self.GuestUI.getToggleValue("createVIPModal-MainTrio");
		data.Personality = self.GuestUI.getMultiSelectValues("createVIPModal-Personality");
		data.ContractFree = self.GuestUI.getToggleValue("createVIPModal-Contract-Free");
		data.ContractExclusive = self.GuestUI.getToggleValue("createVIPModal-Contract-Exclusive");
		data.ContractTrainee = self.GuestUI.getToggleValue("createVIPModal-Contract-Trainee");
		data.HoleExpVirgin = self.GuestUI.getToggleValue("createVIPModal-HoleExp-Virgin");
		data.HoleExpVag = self.GuestUI.getToggleValue("createVIPModal-HoleExp-Vag");
		data.HoleExpAnal = self.GuestUI.getToggleValue("createVIPModal-HoleExp-Anal");
		data.HoleExpBoth = self.GuestUI.getToggleValue("createVIPModal-HoleExp-Both");
		data.Relationship = self.GuestUI.getMultiSelectValues("createVIPModal-Relationship");
		data.Propensity = self.GuestUI.getMultiSelectValues("createVIPModal-Propensity");
		data.JobClass = self.GuestUI.getMultiSelectValues("createVIPModal-JobClass");
		data.NightClass = self.GuestUI.getMultiSelectValues("createVIPModal-NightClass");
	}
	//Rental
	else
	{
		//Basic
		data.RentalMaidName = $("#createVIPModal-RentalMaid").selectpicker('val');
	}
	
	return data;
}
self.CustomScheduleEvents.createNewVIP = function()
{
	self.GuestUI.executeAjax("scheduleVIPCreate");
}

//VIP-Edit
self.CustomScheduleEvents.resetEditVIPModal = function(normalRental, row)
{
	if(row == null)
	{
		$("#editVIPModal-Id").val("");
		$("#editVIPModal-Name").val("");
		self.GuestUI.setDropdownValue("editVIPModal-Icon", 0);
		self.GuestUI.setDropdownValue("editVIPModal-Category", 0);
		$("#editVIPModal-Description").val("");
		$("#editVIPModal-Income").val("");
		for(let i=0; i<9; i++)
		{
			$("#editVIPModal-Hint0"+(i+1)).val("");
		}
		self.GuestUI.setDropdownValue("editVIPModal-SalonGrade",0);
		
		//Normal
		//Basic
		self.GuestUI.setToggleValue("editVIPModal-ImproveRelation", false);
		self.GuestUI.setToggleValue("editVIPModal-EntertainedMaster", false);
		self.GuestUI.setToggleValue("editVIPModal-EntertainedGuest", false);
		//Requirements
		self.GuestUI.setToggleValue("editVIPModal-MainTrio", false);
		self.GuestUI.setMultiSelectValues("editVIPModal-Personality", []);
		self.GuestUI.setToggleValue("editVIPModal-Contract-Free", false);
		self.GuestUI.setToggleValue("editVIPModal-Contract-Exclusive", false);
		self.GuestUI.setToggleValue("editVIPModal-Contract-Trainee", false);
		self.GuestUI.setToggleValue("editVIPModal-HoleExp-Virgin", false);
		self.GuestUI.setToggleValue("editVIPModal-HoleExp-Vag", false);
		self.GuestUI.setToggleValue("editVIPModal-HoleExp-Anal", false);
		self.GuestUI.setToggleValue("editVIPModal-HoleExp-Both", false);
		self.GuestUI.setMultiSelectValues("editVIPModal-Relationship", []);
		self.GuestUI.setMultiSelectValues("editVIPModal-Propensity", []);
		self.GuestUI.setMultiSelectValues("editVIPModal-JobClass", []);
		self.GuestUI.setMultiSelectValues("editVIPModal-NightClass", []);
		
		//Rental
		//Basic
		self.GuestUI.setDropdownValue("editVIPModal-RentalMaid",0);
	}
	else
	{
		var vipId = parseInt(row[0]);
		
		//Send request
		$.ajax(
		{
			type: "POST",
			crossdomain: true,
			url: "http://localhost:8080/",
			data: JSON.stringify({
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"editVIPGetDetails", 
						"parameters":JSON.stringify({Id:vipId})
					}),
			error:[
			function(qXHR, textStatus, errorThrown)
			{
				//Alert
				alert("Error with request, please check Unity Console/Log");
			}],
			success:[
			function(data)
			{
				if(data.error)
				{
					alert(data.ErrorText);
					return;
				}

				//Set data
				editData = JSON.parse(data.data);
				
				$("#editVIPModal-Id").val(editData.Id);
				$("#editVIPModal-Name").val(editData.Name);
				self.GuestUI.setDropdownValue("editVIPModal-Icon", -1, editData.Icon);
				self.GuestUI.setDropdownValue("editVIPModal-Category", -1, editData.CategoryId);
				$("#editVIPModal-Description").val(editData.Description);
				$("#editVIPModal-Income").val(editData.Income);
				for(let i=0; i<9; i++)
				{
					$("#editVIPModal-Hint0"+(i+1)).val("");
					if(i < editData.Hints.length)
					{
						$("#editVIPModal-Hint0"+(i+1)).val(editData.Hints[i]);
					}
				}
				self.GuestUI.setDropdownValue("editVIPModal-SalonGrade",editData.SalonGrade);
				
				//Normal
				//Basic
				self.GuestUI.setToggleValue("editVIPModal-ImproveRelation", editData.ImproveCustomerRelation);
				self.GuestUI.setToggleValue("editVIPModal-EntertainedMaster", editData.EntertainedMaster);
				self.GuestUI.setToggleValue("editVIPModal-EntertainedGuest", editData.EntertainedGuest);
				//Requirements
				self.GuestUI.setToggleValue("editVIPModal-MainTrio", editData.MainTrio);
				self.GuestUI.setMultiSelectValues("editVIPModal-Personality", editData.Personality);
				self.GuestUI.setToggleValue("editVIPModal-Contract-Free", editData.ContractFree);
				self.GuestUI.setToggleValue("editVIPModal-Contract-Exclusive", editData.ContractExclusive);
				self.GuestUI.setToggleValue("editVIPModal-Contract-Trainee", editData.ContractTrainee);
				self.GuestUI.setToggleValue("editVIPModal-HoleExp-Virgin", editData.HoleExpVirgin);
				self.GuestUI.setToggleValue("editVIPModal-HoleExp-Vag", editData.HoleExpVag);
				self.GuestUI.setToggleValue("editVIPModal-HoleExp-Anal", editData.HoleExpAnal);
				self.GuestUI.setToggleValue("editVIPModal-HoleExp-Both", editData.HoleExpBoth);
				self.GuestUI.setMultiSelectValues("editVIPModal-Relationship", editData.Relationship);
				self.GuestUI.setMultiSelectValues("editVIPModal-Propensity", editData.Propensity);
				self.GuestUI.setMultiSelectValues("editVIPModal-JobClass", editData.JobClass);
				self.GuestUI.setMultiSelectValues("editVIPModal-NightClass", editData.NightClass);
				
				//Rental
				//Basic
				self.GuestUI.setDropdownValue("editVIPModal-RentalMaid", -1, editData.RentalMaidName);
				
				if(normalRental == "normal")
				{
					//Hide Rental controls
					$("#editVIPModal-basic-rental").parent().css("display", "none");
					
					//Open
					$('#editVIPModal').modal('show');
				}
				else if(normalRental == "rental")
				{
					//Hide Normal controls
					$("#editVIPModal-basic-normal").parent().css("display", "none");
					$("#editVIPModal-requirements-normal").parent().css("display", "none");
					
					//Open
					$('#editVIPModal').modal('show');
				}
			}]
		});
	}
};
self.CustomScheduleEvents.openEditVIPNormalModal = function(table, row)
{
	//Reset
	self.CustomScheduleEvents.resetEditVIPModal("normal", row);
};
self.CustomScheduleEvents.openEditVIPRentalModal = function(table, row)
{
	//Reset
	self.CustomScheduleEvents.resetEditVIPModal("rental", row);	
};
self.CustomScheduleEvents.closeEditVIPModal = function(id)
{
	if(id!= null)
	{
		if(self.GuestUI.ajaxRequests.reqs[id].hasOwnProperty("respData"))
		{
			alert(self.GuestUI.ajaxRequests.reqs[id].respData);
		}
	}
	//Reset
	self.CustomScheduleEvents.resetEditVIPModal();
	
	//Open
	$('#editVIPModal').modal('hide');
}
self.CustomScheduleEvents.saveEditVIPParams = function()
{
	//Shared
	var data = {};
	data.rental = $("#editVIPModal-basic-normal").parent().css("display") == "none";
	
	data.Id = parseInt($("#editVIPModal-Id").val()) || 99999;
	data.Name = $("#editVIPModal-Name").val() || "";
	data.Icon = $("#editVIPModal-Icon").selectpicker('val');
	data.CategoryId = parseInt($("#editVIPModal-Category").selectpicker('val'));
	data.Description = $("#editVIPModal-Description").val() || "";
	data.Income = parseInt($("#editVIPModal-Income").val()) || 0;
	data.Hints = [];
	for(let i=0; i<9; i++)
	{
		var val = $("#editVIPModal-Hint0"+(i+1)).val();
		if( val!= null && val.trim() != "")
		{
			data.Hints.push(val);
		}
	}
	data.SalonGrade = parseInt($("#editVIPModal-SalonGrade").selectpicker('val'));
	
	//Normal
	if(!data.rental)
	{
		//Basic
		data.ImproveCustomerRelation = self.GuestUI.getToggleValue("editVIPModal-ImproveRelation");
		data.EntertainedMaster = self.GuestUI.getToggleValue("editVIPModal-EntertainedMaster");
		data.EntertainedGuest = self.GuestUI.getToggleValue("editVIPModal-EntertainedGuest");
		
		//Requirements
		data.MainTrio = self.GuestUI.getToggleValue("editVIPModal-MainTrio");
		data.Personality = self.GuestUI.getMultiSelectValues("editVIPModal-Personality");
		data.ContractFree = self.GuestUI.getToggleValue("editVIPModal-Contract-Free");
		data.ContractExclusive = self.GuestUI.getToggleValue("editVIPModal-Contract-Exclusive");
		data.ContractTrainee = self.GuestUI.getToggleValue("editVIPModal-Contract-Trainee");
		data.HoleExpVirgin = self.GuestUI.getToggleValue("editVIPModal-HoleExp-Virgin");
		data.HoleExpVag = self.GuestUI.getToggleValue("editVIPModal-HoleExp-Vag");
		data.HoleExpAnal = self.GuestUI.getToggleValue("editVIPModal-HoleExp-Anal");
		data.HoleExpBoth = self.GuestUI.getToggleValue("editVIPModal-HoleExp-Both");
		data.Relationship = self.GuestUI.getMultiSelectValues("editVIPModal-Relationship");
		data.Propensity = self.GuestUI.getMultiSelectValues("editVIPModal-Propensity");
		data.JobClass = self.GuestUI.getMultiSelectValues("editVIPModal-JobClass");
		data.NightClass = self.GuestUI.getMultiSelectValues("editVIPModal-NightClass");
	}
	//Rental
	else
	{
		//Basic
		data.RentalMaidName = $("#editVIPModal-RentalMaid").selectpicker('val');
	}
	
	return data;
}
self.CustomScheduleEvents.saveEditVIP = function()
{
	self.GuestUI.executeAjax("scheduleVIPEdit");
}

//VIP-Import
self.CustomScheduleEvents.importVIPParams = {};
self.CustomScheduleEvents.importVIP = function(table, row)
{
	self.CustomScheduleEvents.importVIPParams = {Id:row[0], Name:row[1]};
	self.GuestUI.executeAjax("scheduleVIPImport");
}
self.CustomScheduleEvents.saveImportVIPParams = function()
{
	return self.CustomScheduleEvents.importVIPParams;
}
self.CustomScheduleEvents.importedVIP = function(id)
{
	self.CustomScheduleEvents.importVIPParams = {};
	
	if(id!= null)
	{
		if(self.GuestUI.ajaxRequests.reqs[id].hasOwnProperty("respData"))
		{
			alert(self.GuestUI.ajaxRequests.reqs[id].respData);
		}
	}
}

//VIP-Delete
self.CustomScheduleEvents.deleteVIPParams = {};
self.CustomScheduleEvents.deleteVIP = function(table, row)
{
	self.CustomScheduleEvents.deleteVIPParams = {Id:row[0]};
	self.GuestUI.executeAjax("scheduleVIPDelete");
};
self.CustomScheduleEvents.saveDeleteVIPParams = function()
{
	return self.CustomScheduleEvents.deleteVIPParams;
}
self.CustomScheduleEvents.deletedVIP = function(id)
{
	self.CustomScheduleEvents.deleteVIPParams = {};
	
	if(id!= null)
	{
		if(self.GuestUI.ajaxRequests.reqs[id].hasOwnProperty("respData"))
		{
			alert(self.GuestUI.ajaxRequests.reqs[id].respData);
		}
	}
}

//ScriptHelpers
self.CustomScheduleEvents.scriptHelpersMotionScriptFileDropdownChanged = function(e, clickedIndex, isSelected, previousValue)
{
	//Disable all children
	$("#nav-scriptHelpers-pane-motionscript-label").children().prop("disabled", true);
	
	var selectedValue = ($($(this).children()[clickedIndex]).val() != null)? $($(this).children()[clickedIndex]).val().trim() : "";
	
    $("#nav-scriptHelpers-pane-motionscript-label").children().filter(function( index ) 
	{
		return ($(this).val().split("|")[0].trim() == selectedValue);
	}).prop("disabled", false);
	
	//Apply
	$("#nav-scriptHelpers-pane-motionscript-label").selectpicker('refresh');
}
self.CustomScheduleEvents.scriptHelpersMotionScriptTest = function()
{
	//Data
	var data = {};
	data.File = $("#nav-scriptHelpers-pane-motionscript-file").selectpicker('val');
	data.Label = $("#nav-scriptHelpers-pane-motionscript-label").selectpicker('val');

	if (data.Label.split("|").length == 2)
	{
		data.File = data.Label.split("|")[0].trim().Split(".")[0];
		data.Label = data.Label.split("|")[1].trim();
	}

	self.CustomScheduleEvents._scriptHelpersMotionScriptSample(data);
}
self.CustomScheduleEvents.scriptHelpersMotionScriptSample = function(table, row)
{
	var data = {};
	data.File = row[0];
	data.Label = row[2];

	if (data.Label.split("|").length == 2)
	{
		data.File = data.Label.split("|")[0].trim().Split(".")[0];
		data.Label = data.Label.split("|")[1].trim();
	}

	self.CustomScheduleEvents._scriptHelpersMotionScriptSample(data);
};
self.CustomScheduleEvents._scriptHelpersMotionScriptSample = function (data)
{
	//Send request
	var ajaxUrl = "http://localhost:8080/";
	var ajaxDataCopy = {
		"className": "COM3D2.CustomScheduleEvents.Plugin.WebUI",
		"methodName": "MotionScriptTest",
		"parameters": JSON.stringify(data)
	};
	$.ajax({
		type: "POST",
		crossdomain: true,
		url: ajaxUrl,
		data: JSON.stringify(ajaxDataCopy),
		error: [
			function (qXHR, textStatus, errorThrown) {
				//Alert
				alert("Error with request, please check Unity Console/Log");
			}],
		success: [
			function (successData) {
				if (successData.error) {
					window.alert(successData.errorText)
				}
			}]
	});
}
self.CustomScheduleEvents.scriptHelpersMotionSample = function (table, row)
{
	var rowMan = row[1].trim().toLowerCase();
	var dropdownMaid = $("#nav-scriptHelpers-pane-motion-maid").selectpicker('val');
	if (rowMan == "true" && dropdownMaid < 6)
	{
		alert("Cannot use Man animation with Maid");
	}
	if (rowMan == "false" && dropdownMaid >= 6)
	{
		alert("Cannot use Maid animation with Man");
	}

	var data = {};
	data.Man = dropdownMaid >= 6;
	data.Maid = (data.Man) ? (dropdownMaid - 6) : dropdownMaid;
	data.Mot = row[0].trim();
	data.Loop = self.GuestUI.getToggleValue("nav-scriptHelpers-pane-motion-loop-bool");
	data.Blend = self.GuestUI.getToggleValue("nav-scriptHelpers-pane-motion-blend-bool") ? (parseInt($("#nav-scriptHelpers-pane-motion-blend-val").val()) || 500) : 500;
	data.Additive = self.GuestUI.getToggleValue("nav-scriptHelpers-pane-motion-add-bool");
	data.Weight = self.GuestUI.getToggleValue("nav-scriptHelpers-pane-motion-weight-bool") ? (parseInt($("#nav-scriptHelpers-pane-motion-weight-val").val()) || 1000) : 1000;

	self.CustomScheduleEvents._scriptHelpersMotionSample(data);
}
self.CustomScheduleEvents._scriptHelpersMotionSample = function (data)
{
	//Send request
	var ajaxUrl = "http://localhost:8080/";
	var ajaxDataCopy = {
		"className": "COM3D2.CustomScheduleEvents.Plugin.WebUI",
		"methodName": "MotionTest",
		"parameters": JSON.stringify(data)
	};
	$.ajax({
		type: "POST",
		crossdomain: true,
		url: ajaxUrl,
		data: JSON.stringify(ajaxDataCopy),
		error: [
			function (qXHR, textStatus, errorThrown) {
				//Alert
				alert("Error with request, please check Unity Console/Log");
			}],
		success: [
			function (successData) {
				if (successData.error) {
					window.alert(successData.errorText)
				}
			}]
	});
}

self.CustomScheduleEvents.scriptHelpersSoundSample = function (table, row)
{
	var data = {};
	data.File = row[0].trim();

	self.CustomScheduleEvents._scriptHelpersMotionSample(data);
}
self.CustomScheduleEvents._scriptHelpersMotionSample = function (data)
{
	//Send request
	var ajaxUrl = "http://localhost:8080/";
	var ajaxDataCopy = {
		"className": "COM3D2.CustomScheduleEvents.Plugin.WebUI",
		"methodName": "SoundTest",
		"parameters": JSON.stringify(data)
	};
	$.ajax({
		type: "POST",
		crossdomain: true,
		url: ajaxUrl,
		data: JSON.stringify(ajaxDataCopy),
		error: [
			function (qXHR, textStatus, errorThrown) {
				//Alert
				alert("Error with request, please check Unity Console/Log");
			}],
		success: [
			function (successData) {
				if (successData.error) {
					window.alert(successData.errorText)
				}
			}]
	});
}

self.CustomScheduleEvents.renderFavorite = function (data, type)
{
	if (type === 'display')
	{
		if (data.trim() == "1")
		{
			return '<i class="bi bi-star-fill" data-bs-toggle="tooltip" title="How to Search: Unknown = 0, Favorite = 1, Hidden = 2" style="font-size:initial;"></i>'
		}
		else if (data.trim() == "2")
		{
			return '<i class="bi bi-eye-slash-fill" data-bs-toggle="tooltip" title="How to Search: Unknown = 0, Favorite = 1, Hidden = 2" style="font-size:initial;"></i>'
		}
 
		return '<i class="bi bi-question" data-bs-toggle="tooltip" title="How to Search: Unknown = 0, Favorite = 1, Hidden = 2" style="font-size:initial;"></i>'
	}

	if (data.trim() == "1")
	{
		return 'Fav:Favorite'
	}
	else if (data.trim() == "2")
	{
		return 'Fav:Hidden'
	}

	return 'Fav:?'
 
    //return data;
};

self.CustomScheduleEvents.scriptHelpersMotionScriptFavoriteUpdate = function (table, row) {
	var data = {};
	data.table = "motionscript";
	data.rowId = row[4] + "_" + row[2];
	data.status = (row[4] == "0") ? 1 : (row[4] == "1") ? 2 : 0;

	self.CustomScheduleEvents.scriptHelpersSoundFavoriteUpdate(data, table, row);
};
self.CustomScheduleEvents.scriptHelpersMotionFavoriteUpdate = function (table, row) {
	var data = {};
	data.table = "motion";
	data.rowId = row[4] + "_" + row[0];
	data.status = (row[5] == "0") ? 1 : (row[5] == "1") ? 2 : 0;

	self.CustomScheduleEvents.scriptHelpersSoundFavoriteUpdate(data, table, row);
};
self.CustomScheduleEvents.scriptHelpersSoundFavoriteUpdate = function (table, row)
{
	var data = { };
	data.table = "sound";
	data.rowId = row[6] + "_" + row[0];
	data.status = (row[7] == "0") ? 1 : (row[7] == "1") ? 2 : 0;

	self.CustomScheduleEvents.scriptHelpersSoundFavoriteUpdate(data, table, row);
}
self.CustomScheduleEvents.scriptHelpersSoundFavoriteUpdate = function (data, table, row) {

	var s = function (dta, tble, rw) {
		return function (successData) {
			if (!successData.error) {
				let rowIndex = tble.data().indexOf(rw);
				let colIndex = tble.column(':contains(Favorite)').index();
				tble.cell({ row: rowIndex, column: colIndex }).data(dta['status'].toString());
			}
		}
	};

	//Send request
	var ajaxUrl = "http://localhost:8080/";
	var ajaxDataCopy = {
		"className": "COM3D2.CustomScheduleEvents.Plugin.WebUI",
		"methodName": "ScriptTestTableFavoriteUpdate",
		"parameters": JSON.stringify(data)
	};
	$.ajax({
		type: "POST",
		crossdomain: true,
		url: ajaxUrl,
		data: JSON.stringify(ajaxDataCopy),
		error: [
			function (qXHR, textStatus, errorThrown) {
				//Alert
				alert("Error with request, please check Unity Console/Log");
			}],
		success: [
			s(data, table, row),
			function (successData) {
				debugger;
				if (successData.error) {
					window.alert(successData.errorText)
				}
			}]
	});
}

self.CustomScheduleEvents.initialize = function()
{
	//Build HTML
	var html = 
	{
		header: "CustomScheduleEvents",
		pages:
		[
			{
				id:"nav-home", 
				name:"Home", 
				header:
				{
					label:"Home", 
					buttons:[]
				}, 
				contents:
				[
					{
						id:"nav-home-pane",
						label:"",
						rows:[
						[
							{type:"label", label:"Welcome to CustomScheduleEvents", width:12},
						]]
					}
				]
			},
			{
				id:"nav-category", 
				name:"Category", 
				header:
				{
					label:"Category", 
					buttons:
					[
						{
							label:"Create", 
							onclick:self.CustomScheduleEvents.openNewCategoryModal
						},
						{
							label:"Refresh", 
							onclick:self.GuestUI.refreshDataSources
						}
					]
				}, 
				contents:
				[
					{
						id:"nav-category-pane",
						label:"",
						rows:[
						[
							{
								type:"table",
								id:"nav-category-pane-table",
								label:"Categories",
								dataSource:"scheduleCategoriesTableDS",
								options:
								{
									columns:
									[
										{
											title:"ID",
											type:"num"
										},
										{
											title:"Name"
										},
										{
											type:"button",
											name:"Edit",
											label:"Edit",
											event:self.CustomScheduleEvents.openEditCategoryModal
										}
									]
								}
							}
						]]
					}
				],
				modals:
				[
					{
						id:"createCategoryModal",
						label:"Create Category",
						contents:
						[
							{
								id:"createCategoryModal-basic",
								label:"",
								rows:[
								[
									{type:"input", id:"createCategoryModal-Id", label:"ID", width:12, validation:"integer", maxlength:5},
									{type:"input", id:"createCategoryModal-Name", label:"Name", width:12, validation:"alphaeng", maxlength:20},
								]]
							}
						],
						footerButtons:
						[
							{label: "Save", onclick:self.CustomScheduleEvents.createNewCategory}
						]
					},
                    {
                        id:"editCategoryModal",
                        label:"Edit Category",
                        contents:
                        [
                            {
                                id:"editCategoryModal-basic",
                                label:"",
                                rows:[
                                [
                                    {type:"input", id:"editCategoryModal-Id", label:"ID", width:12, validation:"integer", maxlength:5, enabled: false},
									{type:"input", id:"editCategoryModal-Name", label:"Name", width:12, validation:"alphaeng", maxlength:20},
                                ]]
                            }
                        ],
						footerButtons:
						[
							{label: "Save", onclick:self.CustomScheduleEvents.saveEditCategory}
						]
                    }
				]
			},
			{
				id:"nav-vip", 
				name:"VIP", 
				header:
				{
					label:"VIP", 
					buttons:
					[
						{
							label:"Create Normal", 
							onclick:self.CustomScheduleEvents.openNewVIPModalNormal
						},
						{
							label:"Create Rental", 
							onclick:self.CustomScheduleEvents.openNewVIPModalRental
						},
						{
							label:"Refresh", 
							onclick:self.GuestUI.refreshDataSources
						}
					]
				}, 
				contents:
				[
					{
						id:"nav-vip-pane",
						label:"",
						rows:[
						[
							{
								type:"table",
								id:"nav-vip-pane-table-normal",
								label:"VIP Events - Normal",
								dataSource:"scheduleVIPsNormalDS",
								options:
								{
									columns:
									[
										{
											title:"ID",
											type:"num"
										},
										{
											title:"Name"
										},
										{
											title:"Category"
										},
										{
											title:"Description"
										},
										{
											title:"Source"
										},
										{
											type:"button",
											name:"Edit",
											label:"Edit",
											event:self.CustomScheduleEvents.openEditVIPNormalModal
										},
										{
											type:"button",
											name:"Delete",
											label:"Delete",
											event:self.CustomScheduleEvents.deleteVIP
										}
									]
								}
							},
							{
								type:"table",
								id:"nav-vip-pane-table-rental",
								label:"VIP Events - Rental",
								dataSource:"scheduleVIPsRentalDS",
								options:
								{
									columns:
									[
										{
											title:"ID",
											type:"num"
										},
										{
											title:"Name"
										},
										{
											title:"Category"
										},
										{
											title:"Description"
										},
										{
											title:"Source"
										},
										{
											type:"button",
											name:"Edit",
											label:"Edit",
											event:self.CustomScheduleEvents.openEditVIPRentalModal
										},
										{
											type:"button",
											name:"Delete",
											label:"Delete",
											event:self.CustomScheduleEvents.deleteVIP
										}
									]
								}
							},
							{
								type:"table",
								id:"nav-vip-pane-table-import",
								label:"VIP Events - Import",
								dataSource:"scheduleVIPsImportDS",
								options:
								{
									columns:
									[
										{
											title:"ID",
											type:"num"
										},
										{
											title:"Name"
										},
										{
											title:"File",
										},
										{
											title: ""
										},
										{
											title: ""
										},
										{
											title: ""
										},
										{
											type:"button",
											name:"Import",
											label:"Import",
											event:self.CustomScheduleEvents.importVIP
										}
									]
								}
							}
						]]
					}
				],
				modals:
				[
					{
						id:"createVIPModal",
						label:"Create VIP",
						contents:
						[
							{
								id:"createVIPModal-basic",
								label:"Basic",
								rows:[
								[
									{type:"input", id:"createVIPModal-Id", label:"ID", width:12, validation:"integer", maxlength:5},
									{type:"input", id:"createVIPModal-Name", label:"Name", width:12, validation:"alphaeng", maxlength:20},
									{type:"dropdown", id:"createVIPModal-Category", label:"Category", width:4, options:[], dataSource: "scheduleCategoriesDropdownDS"},
									{type:"dropdown", id:"createVIPModal-Icon", label:"Icon", width:4, imgW:80, imgH:80, options:[], dataSource: "scheduleIconsDropdownDS"},
									{type:"input", id:"createVIPModal-Income", label:"Base Income (Usually 0...)", width:4, validation:"integer", maxlength:3},
									{type:"input", id:"createVIPModal-Description", label:"Description", width:12, validation:"alphaeng", maxlength:60},
								]]
							},
							{
								id:"createVIPModal-basic-normal",
								label:"Basic (Normal VIP)",
								rows:[
								[
									{type:"toggle", id:"createVIPModal-ImproveRelation", label:"Improve Maid Customer Relation", width:4},
									{type:"toggle", id:"createVIPModal-EntertainedMaster", label:"Count as Master Entertained", width:4},
									{type:"toggle", id:"createVIPModal-EntertainedGuest", label:"Count as Guest Entertained", width:4}
								]]
							},
							{
								id:"createVIPModal-basic-rental",
								label:"Basic (Rental VIP)",
								rows:[
								[
									{type:"dropdown", id:"createVIPModal-RentalMaid", label:"Rental Maid", width:4, options:[], dataSource: "scheduleRentalMaidsDropdownDS"},
									{type:"space", width:8}
								]]
							},
							{
								id:"createVIPModal-requirements",
								label:"Requirements",
								rows:[
								[
									{type:"label", label:"Hints", width:12},
									{type:"input", id:"createVIPModal-Hint01", label:"Hint #01", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint02", label:"Hint #02", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint03", label:"Hint #03", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint04", label:"Hint #04", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint05", label:"Hint #05", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint06", label:"Hint #06", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint07", label:"Hint #07", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint08", label:"Hint #08", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"createVIPModal-Hint09", label:"Hint #09", width:4, validation:"alphaeng", maxlength:30},
									{type:"dropdown", id:"createVIPModal-SalonGrade", label:"Salon Grade", width:6, options:
									[
										{value: 0, label: "0"}, 
										{value: 1, label: "*"}, 
										{value: 2, label: "**"}, 
										{value: 3, label: "***"}, 
										{value: 4, label: "****"}, 
										{value: 5, label: "*****"}
									]},
									{type:"space", width:6}
								]]
							},
							{
								id:"createVIPModal-requirements-normal",
								label:"Requirements (Normal VIP)",
								rows:[
								[
									{type:"multiselect", id:"createVIPModal-Personality", label:"Maid Personality", width:6, options:[], dataSource: "schedulePersonalityMultiselectDS"},
									{type:"toggle", id:"createVIPModal-MainTrio", label:"Main Trio", width:6},
									{type:"multiselect", id:"createVIPModal-Relationship", label:"Maid Relationship", width:6, options:[
										{label: "Contact", value:"Contact"}, 
										{label: "Trust", value:"Trust"}, 
										{label: "Lover", value:"Lover"}, 
										{label: "Vigilant", value:"Vigilance"}, 
										{label: "Lover Plus", value:"LoverPlus"}, 
										{label: "Slave", value:"Slave"}, 
										{label: "Married", value:"Married"}]},
									{type:"multiselect", id:"createVIPModal-Propensity", label:"Maid Propensity", width:6, options:[], dataSource: "schedulePropensityMultiselectDS"},
									{type:"multiselect", id:"createVIPModal-JobClass", label:"Maid Job Class", width:6, options:[], dataSource: "scheduleJobClassMultiselectDS"},
									{type:"multiselect", id:"createVIPModal-NightClass", label:"Maid Night Class", width:6, options:[], dataSource: "scheduleNightClassMultiselectDS"},
									{type:"multitogglerow", id:"createVIPModal-Contract", label:"Contract", options:[{id:"createVIPModal-Contract-Trainee", label:"Trainee"},{id:"createVIPModal-Contract-Exclusive", label:"Exclusive"},{id:"createVIPModal-Contract-Free", label:"Free"}]},
									{type:"multitogglerow", id:"createVIPModal-HoleExp", label:"Hole Experience", options:[{id:"createVIPModal-HoleExp-Virgin", label:"Virgin"},{id:"createVIPModal-HoleExp-Vag", label:"Vaginal Only"},{id:"createVIPModal-HoleExp-Anal", label:"Anal Only"},{id:"createVIPModal-HoleExp-Both", label:"Both Holes"}]},
								]]
							},
							{
								id:"createVIPModal-advanced",
								label:"Advanced",
								rows:[
								[
									{type:"label", label:"Coming soon...", width:12}
								]]
							}
						],
						footerButtons:
						[
							{label: "Save", onclick:self.CustomScheduleEvents.createNewVIP}
						]
					},
					{
						id:"editVIPModal",
						label:"Edit VIP",
						contents:
						[
							{
								id:"editVIPModal-basic",
								label:"Basic",
								rows:[
								[
									{type:"input", id:"editVIPModal-Id", label:"ID", width:12, validation:"integer", maxlength:5, enabled:false},
									{type:"input", id:"editVIPModal-Name", label:"Name", width:12, validation:"alphaeng", maxlength:20},
									{type:"dropdown", id:"editVIPModal-Category", label:"Category", width:4, options:[], dataSource: "scheduleCategoriesDropdownDS"},
									{type:"dropdown", id:"editVIPModal-Icon", label:"Icon", width:4, imgW:80, imgH:80, options:[], dataSource: "scheduleIconsDropdownDS"},
									{type:"input", id:"editVIPModal-Income", label:"Base Income (Usually 0...)", width:4, validation:"integer", maxlength:3},
									{type:"input", id:"editVIPModal-Description", label:"Description", width:12, validation:"alphaeng", maxlength:60},
								]]
							},
							{
								id:"editVIPModal-basic-normal",
								label:"Basic (Normal VIP)",
								rows:[
								[
									{type:"toggle", id:"editVIPModal-ImproveRelation", label:"Improve Maid Customer Relation", width:4},
									{type:"toggle", id:"editVIPModal-EntertainedMaster", label:"Count as Master Entertained", width:4},
									{type:"toggle", id:"editVIPModal-EntertainedGuest", label:"Count as Guest Entertained", width:4}
								]]
							},
							{
								id:"editVIPModal-basic-rental",
								label:"Basic (Rental VIP)",
								rows:[
								[
									{type:"dropdown", id:"editVIPModal-RentalMaid", label:"Rental Maid", width:4, options:[], dataSource: "scheduleRentalMaidsDropdownDS"},
									{type:"space", width:8}
								]]
							},
							{
								id:"editVIPModal-requirements",
								label:"Requirements",
								rows:[
								[
									{type:"label", label:"Hints", width:12},
									{type:"input", id:"editVIPModal-Hint01", label:"Hint #01", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint02", label:"Hint #02", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint03", label:"Hint #03", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint04", label:"Hint #04", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint05", label:"Hint #05", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint06", label:"Hint #06", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint07", label:"Hint #07", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint08", label:"Hint #08", width:4, validation:"alphaeng", maxlength:30},
									{type:"input", id:"editVIPModal-Hint09", label:"Hint #09", width:4, validation:"alphaeng", maxlength:30},
									{type:"dropdown", id:"editVIPModal-SalonGrade", label:"Salon Grade", width:6, options:
									[
										{value: 0, label: "0"}, 
										{value: 1, label: "*"}, 
										{value: 2, label: "**"}, 
										{value: 3, label: "***"}, 
										{value: 4, label: "****"}, 
										{value: 5, label: "*****"}
									]},
									{type:"space", width:6}
								]]
							},
							{
								id:"editVIPModal-requirements-normal",
								label:"Requirements (Normal VIP)",
								rows:[
								[
									{type:"multiselect", id:"editVIPModal-Personality", label:"Maid Personality", width:6, options:[], dataSource: "schedulePersonalityMultiselectDS"},
									{type:"toggle", id:"editVIPModal-MainTrio", label:"Main Trio", width:6},
									{type:"multiselect", id:"editVIPModal-Relationship", label:"Maid Relationship", width:6, options:[
										{label: "Contact", value:"Contact"}, 
										{label: "Trust", value:"Trust"}, 
										{label: "Lover", value:"Lover"}, 
										{label: "Vigilant", value:"Vigilance"}, 
										{label: "Lover Plus", value:"LoverPlus"}, 
										{label: "Slave", value:"Slave"}, 
										{label: "Married", value:"Married"}]},
									{type:"multiselect", id:"editVIPModal-Propensity", label:"Maid Propensity", width:6, options:[], dataSource: "schedulePropensityMultiselectDS"},
									{type:"multiselect", id:"editVIPModal-JobClass", label:"Maid Job Class", width:6, options:[], dataSource: "scheduleJobClassMultiselectDS"},
									{type:"multiselect", id:"editVIPModal-NightClass", label:"Maid Night Class", width:6, options:[], dataSource: "scheduleNightClassMultiselectDS"},
									{type:"multitogglerow", id:"editVIPModal-Contract", label:"Contract", options:[{id:"editVIPModal-Contract-Trainee", label:"Trainee"},{id:"editVIPModal-Contract-Exclusive", label:"Exclusive"},{id:"editVIPModal-Contract-Free", label:"Free"}]},
									{type:"multitogglerow", id:"editVIPModal-HoleExp", label:"Hole Experience", options:[{id:"editVIPModal-HoleExp-Virgin", label:"Virgin"},{id:"editVIPModal-HoleExp-Vag", label:"Vaginal Only"},{id:"editVIPModal-HoleExp-Anal", label:"Anal Only"},{id:"editVIPModal-HoleExp-Both", label:"Both Holes"}]},
								]]
							},
							{
								id:"editVIPModal-advanced",
								label:"Advanced",
								rows:[
								[
									{type:"label", label:"Coming soon...", width:12}
								]]
							}
						],
						footerButtons:
						[
							{label: "Save", onclick:self.CustomScheduleEvents.saveEditVIP}
						]
					}
				]
			},
			{
				id:"nav-storyYotogi", 
				name:"Story Yotogi", 
				header:
				{
					label:"Story Yotogi", 
					buttons:[]
				}, 
				contents:
				[
					{
						id: "nav-storyYotogi-pane",
						label: "",
						rows: [
							[
								{ type: "label", label: "Coming soon...", width: 12 },
							]]
					}
				] 
			},
			{
				id:"nav-scriptHelpers", 
				name:"Script Helpers", 
				header:
				{
					label:"Script Helpers", 
					buttons:[]
				}, 
				contents:
				[
					{
						id: "nav-scriptHelpers-pane",
						label: "",
						rows: [
							[
								{ type: "label", label: "MotionScript", width: 12 },
								//{ type: "dropdown", id: "nav-scriptHelpers-pane-motionscript-file", label: "File", width: 4, options: [], liveSearch:true, dataSource: "scriptHelpersMotionScriptFileDropdownDS", onchange: self.CustomScheduleEvents.scriptHelpersMotionScriptFileDropdownChanged},
								//{ type: "space", width: 1 },
								//{ type: "dropdown", id: "nav-scriptHelpers-pane-motionscript-label", label: "Label", width: 4, options: [], dataSource: "scriptHelpersMotionScriptLabelDropdownDS" },
								//{ type: "space", width: 1 },
								//{ type: "button", id: "nav-scriptHelpers-pane-motionscript-test", label: "Test", width: 2, onclick: self.CustomScheduleEvents.scriptHelpersMotionScriptTest },
								{
									type: "table", id: "nav-scriptHelpers-pane-motionscript-table", label: "MotionScript Sampler", dataSource: "scriptHelpersMotionScriptTableDS",
									options: { columns: [{ title: "File" }, { title: "File Translated (Tags)" }, { title: "Action" }, { title: "Action Translated" }, { title: "Favorite", render: self.CustomScheduleEvents.renderFavorite, searchBuilder: { orthogonal: { display: 'filter', search:'filter' } } }, {type: "button", name:"Favorite", label:"Favorite", event:self.CustomScheduleEvents.scriptHelpersMotionScriptFavoriteUpdate}, { type: "button", name: "Sample", label: "Sample", event: self.CustomScheduleEvents.scriptHelpersMotionScriptSample}] }
								},
								{ type: "label", label: "Motion", width: 12 },
								{
									type: "dropdown", id: "nav-scriptHelpers-pane-motion-maid", label: "Maid/Man", width: 4, options:
										[
											{ value: 0, label: "Maid 1" },
											{ value: 1, label: "Maid 2" },
											{ value: 2, label: "Maid 3" },
											{ value: 3, label: "Maid 4" },
											{ value: 4, label: "Maid 5" },
											{ value: 5, label: "Maid 6" },
											{ value: 6, label: "Man 1" },
											{ value: 7, label: "Man 2" },
											{ value: 8, label: "Man 3" },
											{ value: 9, label: "Man 4" },
											{ value: 10, label: "Man 5" },
											{ value: 11, label: "Man 6" }
										]
								},
								{ type: "toggle", id: "nav-scriptHelpers-pane-motion-loop-bool", label: "Loop", width: 2 },
								{ type: "toggle", id: "nav-scriptHelpers-pane-motion-blend-bool", label: "Blend", width: 2 },
								{ type: "input", id: "nav-scriptHelpers-pane-motion-blend-val", label: "Blend Time (default: 500)", width: 4, validation: "integer", maxlength: 5 },
								{ type: "space", width: 4 },
								{ type: "toggle", id: "nav-scriptHelpers-pane-motion-add-bool", label: "Additive", width: 2 },
								{ type: "toggle", id: "nav-scriptHelpers-pane-motion-weight-bool", label: "Weight", width: 2 },
								{ type: "input", id: "nav-scriptHelpers-pane-motion-weight-val", label: "Weight Value (default: 1000)", width: 4, validation: "integer", maxlength: 5 },
								{
									type: "table", id: "nav-scriptHelpers-pane-motion-table", label: "Motion Sampler", dataSource: "scriptHelpersMotionTableDS",
									options: { columns: [{ title: "Motion" }, { title: "Male" }, { title: "Example Script" }, { title: "Example" }, { title: "Game" }, { title: "Favorite", render: self.CustomScheduleEvents.renderFavorite, searchBuilder: { orthogonal: { display: 'filter', search: 'filter' } } }, { type: "button", name: "Favorite", label: "Favorite", event: self.CustomScheduleEvents.scriptHelpersMotionFavoriteUpdate },{ type: "button", name: "Sample", label: "Sample", event: self.CustomScheduleEvents.scriptHelpersMotionSample }] }
								},
								{ type: "label", label: "Sound", width: 12 },
								{
									type: "table", id: "nav-scriptHelpers-pane-sound-table", label: "Sound Sampler", dataSource: "scriptHelpersSoundTableDS",
									options: { columns: [{ title: "File" }, { title: "Sound Type" }, { title: "Subtitles" }, { title: "Subtitles (Translated)" }, { title: "Example Script" }, { title: "Example" }, { title: "Game" }, { title: "Favorite", render: self.CustomScheduleEvents.renderFavorite, searchBuilder: { orthogonal: { display: 'filter', search: 'filter' } } }, { type: "button", name: "Favorite", label: "Favorite", event: self.CustomScheduleEvents.scriptHelpersSoundFavoriteUpdate },{ type: "button", name: "Sample", label: "Sample", event: self.CustomScheduleEvents.scriptHelpersSoundSample }] }
								},
							]
						]
					}
				] 
			}
		],
		ajax:{
			url:"http://localhost:8080/",
			reqs:[
				{
					id: "scheduleCategoriesTableDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleCategoriesTableData", 
						"parameters":""
					}
				},
				{
					id:"scheduleCategoriesCreate",
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"saveNewCategory", 
						"parameters":self.CustomScheduleEvents.createNewCategoryParams
					},
					callbacks:{
						"success":[self.CustomScheduleEvents.closeNewCategoryModal, self.GuestUI.refreshDataSources],
						"error": [self.CustomScheduleEvents.ajaxError]
					}
				},
				{
					id:"scheduleCategoriesEdit",
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI",
						"methodName":"saveEditCategory",
						"parameters":self.CustomScheduleEvents.saveEditCategoryParams
					},
					callbacks:{
						"success":[self.CustomScheduleEvents.closeEditCategoryModal, self.GuestUI.refreshDataSources],
						"error": [self.CustomScheduleEvents.ajaxError]
					}
				},
				{
					id: "scheduleVIPsNormalDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleVIPNormalTableData", 
						"parameters":""
					}
				},
				{
					id: "scheduleVIPsRentalDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleVIPRentalTableData", 
						"parameters":""
					}
				},
				{
					id: "scheduleVIPsImportDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleVIPImportTableData", 
						"parameters":""
					}
				},
				{
					id: "scheduleCategoriesDropdownDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleCategoriesDropdownData", 
						"parameters":""
					}
				},
				{
					id: "scheduleIconsDropdownDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleIconsDropdownData", 
						"parameters":""
					}
				},
				{
					id: "schedulePersonalityMultiselectDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getSchedulePersonalityMultiselectData", 
						"parameters":""
					}
				},
				{
					id: "schedulePropensityMultiselectDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getSchedulePropensityMultiselectData", 
						"parameters":""
					}
				},
				{
					id: "scheduleJobClassMultiselectDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleJobClassMultiselectData", 
						"parameters":""
					}
				},
				{
					id: "scheduleNightClassMultiselectDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleNightClassMultiselectData", 
						"parameters":""
					}
				},
				{
					id: "scheduleRentalMaidsDropdownDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScheduleRentalMaidsDropdownData", 
						"parameters":""
					}
				},
				{
					id:"scheduleVIPCreate",
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"saveNewVIP", 
						"parameters":self.CustomScheduleEvents.createNewVIPParams
					},
					callbacks:{
						"success":[self.CustomScheduleEvents.closeNewVIPModal, self.GuestUI.refreshDataSources],
						"error": [self.CustomScheduleEvents.ajaxError]
					}
				},
				{
					id:"scheduleVIPEdit",
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"saveEditVIP", 
						"parameters":self.CustomScheduleEvents.saveEditVIPParams
					},
					callbacks:{
						"success":[self.CustomScheduleEvents.closeEditVIPModal, self.GuestUI.refreshDataSources],
						"error": [self.CustomScheduleEvents.ajaxError]
					}
				},
				{
					id:"scheduleVIPImport",
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"importVIP", 
						"parameters":self.CustomScheduleEvents.saveImportVIPParams
					},
					callbacks:{
						"success":[self.CustomScheduleEvents.importedVIP, self.GuestUI.refreshDataSources],
						"error": [self.CustomScheduleEvents.ajaxError]
					}
				},
				{
					id:"scheduleVIPDelete",
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"deleteVIP", 
						"parameters":self.CustomScheduleEvents.saveDeleteVIPParams
					},
					callbacks:{
						"success":[self.CustomScheduleEvents.deletedVIP, self.GuestUI.refreshDataSources],
						"error": [self.CustomScheduleEvents.ajaxError]
					}
				},
				{
					id: "scriptHelpersMotionScriptFileDropdownDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScriptHelpersMotionScriptFileDropdownData", 
						"parameters":""
					}
				},
				{
					id: "scriptHelpersMotionScriptLabelDropdownDS", 
					type:"datasource", 
					params:{
						"className":"COM3D2.CustomScheduleEvents.Plugin.WebUI", 
						"methodName":"getScriptHelpersMotionScriptLabelDropdownData", 
						"parameters":""
					}
				},
				{
					id: "scriptHelpersMotionScriptTableDS",
					type: "datasource",
					params: {
						"className": "COM3D2.CustomScheduleEvents.Plugin.WebUI",
						"methodName": "getScriptHelpersMotionScriptTableData",
						"parameters": ""
                    }
				},
				{
					id: "scriptHelpersMotionTableDS",
					type: "datasource",
					params: {
						"className": "COM3D2.CustomScheduleEvents.Plugin.WebUI",
						"methodName": "getScriptHelpersMotionTableData",
						"parameters": ""
					}
				},
				{
					id: "scriptHelpersSoundTableDS",
					type: "datasource",
					params: {
						"className": "COM3D2.CustomScheduleEvents.Plugin.WebUI",
						"methodName": "getScriptHelpersSoundData",
						"parameters": ""
					}
				}
			]
		}
	}
	self.GuestUI.createMain(html);
}