$(document).ready(function(){
    $("#draw-button").unbind("click");
    $("#draw-button").bind("click", WinnerDraw);
	$("#bonus-draw-button").unbind("click");
    $("#bonus-draw-button").bind("click", BonusDraw);
	$("#winner-list-btn").unbind("click");
    $("#winner-list-btn").bind("click", WinnerList);
	$("#present-list").on('changed.bs.select', function () {
        GetPresentAmount($(this).val())
    })
	$("#delete-winner-button").unbind("click");
    $("#delete-winner-button").bind("click", AddCheckBox);
	$(".selectpicker").selectpicker();
});
function GetPresentAmount(presentAmount){
	$("#present-amount").html("")
	for(var index = 1 ; index <= presentAmount ; index ++){
		$("#present-amount").append('<option>' + 
		index +
		'</option>')
	}
	$("#present-amount").selectpicker("refresh");
}
function AddCheckBox() {
    decisionMessage("確定要清除嗎?", "ClearWinnerList()");
}
function ClearWinnerList(){
	$.ajax({
        method:"POST",
        url:"/Home/ClearWinnerList",
        dataType:"json",
        success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			alert(response.message)
        },        
		error: function (error) {
        },
    })
}
function WinnerList(){
	$.ajax({
        method:"POST",
        url:"/Home/WinnerList",
        dataType:"json",
        success: function (response) {
			var winner = ""
			$.each(response,function(index,value){
				winner = winner + value.empNumber +" - "+ value.empName +" - "+ value.department + "-" +value.present + "\r\n";
			})
			$("#winner-list").text(winner)
        },        
		error: function (error) {
        },
    })
	$('#winner-list-modal').modal("show")
}
function WinnerDraw(){
	var presentInfo = $("#present-list").find("option:selected").text()
	var present 	= presentInfo.split("-")[0]
	var sponsor 	= presentInfo.split("-")[1]
	var amount 		= $("#present-amount").find("option:selected").text()
	if( presentInfo == null || presentInfo 	== "" ||
		amount  	== null || amount  		== "" ){
		alert("獎項或數量未選擇")
		return;
	}
	$.ajax({
        method:"POST",
        url:"/Home/WinnerDraw",
        dataType:"json",
        data: {
			present:present,
			sponsor:sponsor,
			amount:amount
        },
        success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			var winner = ""
			$.each(response.responseData.winner,function(index,value){
				winner = winner + value.empNumber +" - "+ value.empName +" - "+ value.department +"\r\n";
			})
			alert(response.message)
			$("#winner-area").text(winner)
        },        
		error: function (error) {
            //alert("資料沒帶出來");
        },
    });
}
function BonusDraw(){
	var present 	= $("#bonus-present").val().trim();
	var amount 		= $("#bonus-amount").val().trim();
	var sponsor 	= $("#bonus-sponsor").val().trim();
	if( present == null || present == "" ||
		amount  == null || amount  == "" ||
		sponsor == null || sponsor == "" ){
		alert("欄位不能空白歐")
		return;
	}
	$.ajax({
        method:"POST",
        url:"/Home/BonusDraw",
        dataType:"json",
        data: {
			present:present,
			sponsor:sponsor,
			amount:amount
        },
		success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			var winner = ""
			$.each(response.responseData.winner,function(index,value){
				winner = winner + value.empNumber +" - "+ value.empName +" - "+ value.department +"\r\n";
			})
			alert(response.message)
			$("#bonus-winner-area").text(winner)
        },        
		error: function (error) {
            //alert("資料沒帶出來");
        },
    });
}