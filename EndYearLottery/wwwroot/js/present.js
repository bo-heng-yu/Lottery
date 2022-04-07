$(document).ready(function(){
	$("#add-present-btn").unbind("click");
    $("#add-present-btn").bind("click", function(){
		$('#add-present-modal').modal("show")
	});	
	$(".present-edit").unbind("click");
    $(".present-edit").bind("click", function(){
		var presentId = $(this).val();
		GetPresentRowInfo(presentId);
	});
	$("#edit-present-comfirm").unbind("click");
    $("#edit-present-comfirm").bind("click", EditPresent);
	$("#add-present-comfirm").unbind("click");
    $("#add-present-comfirm").bind("click", AddPresent);
	$(".present-delete").unbind("click");
    $(".present-delete").bind("click", function(){
		var presentId = $(this).val();
		var present 	= $("#row-present-"+presentId).find(".present").text()
		DeleteCheckBox(present,presentId);
	});
	$("#present-upload-btn").unbind("click");
    $("#present-upload-btn").bind("click", ImportCheckBox);	
});
function GetPresentRowInfo(presentId){
	var row 	= $("#row-present-"+presentId)
	var present = row.find(".present").text()
	var sponsor = row.find(".sponsor").text()
	var amount 	= row.find(".amount").text()
	$("#edit-present").val(present)
	$("#edit-sponsor").val(sponsor)
	$("#edit-present-amount").val(amount)
	$("#edit-present-id").val(presentId)
	$('#edit-present-modal').modal("show")
}
function EditPresent(){
	var presentId 	= $("#edit-present-id").val()
	var present		= $("#edit-present").val()
	var sponsor		= $("#edit-sponsor").val()
	var amount		= $("#edit-present-amount").val()
	if( present == null || present == "" ||
		sponsor == null || sponsor == "" ||
		amount  == null || amount  == ""){
		alert("欄位不能空白歐")
		return;
	}
	$.ajax({
        method:"POST",
        url:"/Home/EditPresent",
        dataType:"json",
		data:{
			presentId:presentId,
            present: present,
			sponsor: sponsor,
			amount: amount,
        },
        success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			var row 	= $("#row-present-"+presentId)
			row.find(".present").text(present)
			row.find(".sponsor").text(sponsor)
			row.find(".amount").text(amount)
			$('#edit-present-modal').modal("hide")
			alert(response.message)
        },        
		error: function (error) {
        },
    })
}
function AddPresent(){
	var present 	= $("#add-present").val().trim();
	var sponsor 	= $("#add-sponsor").val().trim();
	var amount 		= $("#add-present-amount").val().trim();
	if( present == null || present == "" ||
		sponsor == null || sponsor == "" ||
		amount  == null || amount  == ""){
		alert("欄位不能空白歐")
		return;
	}
	$.ajax({
        method:"POST",
        url:"/Home/AddPresent",
        dataType:"json",
		data:{
            present: present,
			sponsor: sponsor,
			amount: amount,
        },
        success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			alert(response.message)	
			location.reload()
        },        
		error: function (error) {
        },
    })
}
function DeleteCheckBox(present,presentId) {
    decisionMessage("確定要刪除" + present + "嗎?", "DeletePresent(" + presentId + ")");
}
function DeletePresent(presentId){
	$.ajax({
        method:"POST",
        url:"/Home/DeletePresent",
        dataType:"json",
		data:{presentId ,presentId},
        success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			alert(response.message)	
			$("#row-present-"+presentId).remove()
        },        
		error: function (error) {
        },
    })
}
function ImportCheckBox() {
    decisionMessage("確定要匯入嗎?", "ImportPresentExcelToSQL()");
}
function ImportPresentExcelToSQL(){
	var formData = new FormData()
	var presentExcel    	= document.getElementById("present-upload-file").files[0];
	if(presentExcel == null || presentExcel == ""){
		alert("尚未選擇獎項excel")
		return
	}
	formData.append("presentExcel",presentExcel)
	$.ajax({
        method:"POST",
        url:"/Home/ImportPresentExcel",
        dataType:"json",
		processData: false, //使用FormData
        contentType: false, //使用FormData
		data : formData,
        success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			alert(response.message)	
			location.reload()
        },        
		error: function (error) {
        },
    })
}
