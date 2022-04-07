$(document).ready(function(){	
	$("#add-employee-btn").unbind("click");
    $("#add-employee-btn").bind("click", function(){
		$('#add-employee-modal').modal("show")
	});
	$("#add-employee-comfirm").unbind("click");
    $("#add-employee-comfirm").bind("click", AddEmployee);

	$(".employee-delete").unbind("click");
    $(".employee-delete").bind("click", function(){
		var empNumber = $(this).val();
		var empName 	= $("#row-employee-"+empNumber).find(".empName").text()
		DeleteCheckBox(empName,empNumber);
	});
	$("#employee-upload-btn").unbind("click");
    $("#employee-upload-btn").bind("click", ImportCheckBox);
	
});
//員工清單區
function AddEmployee(){
	var empNumber 	= $("#add-employee-number").val().trim();
	var empName 	= $("#add-employee-name").val().trim();
	var jobTitle 	= $("#add-employee-jobtitle").val().trim();
	var department 	= $("#add-employee-department").val().trim();
	if( empNumber == null || empNumber == "" ||
		empName  == null || empName  == "" ||
		department  == null || department  == "" ||
		jobTitle == null || jobTitle == "" ){
		alert("欄位不能空白歐")
		return;
	}
	$.ajax({
        method:"POST",
        url:"/Home/AddEmployee",
        dataType:"json",
		data:{
            empNumber: empNumber,
			empName: empName,
			jobTitle: jobTitle,
			department: department
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
function DeleteCheckBox(empName,empNumber) {
    decisionMessage("確定要刪除 人員'" + empName + "'嗎?", "DeleteEmployee('" + empNumber + "')");
}
function DeleteEmployee(empNumber){
	$.ajax({
        method:"POST",
        url:"/Home/DeleteEmployee",
        dataType:"json",
		data:{empNumber ,empNumber},
        success: function (response) {
			if(response.status == "fail"){
				alert(response.message);
				return;
			}
			alert(response.message)	
			$("#row-employee-"+empNumber).remove()
        },        
		error: function (error) {
        },
    })
}
function ImportCheckBox() {
    decisionMessage("確定要匯入嗎?", "ImportEmployeeExcelToSQL()");
}
function ImportEmployeeExcelToSQL(){
	var formData = new FormData()
	var employeeExcel    	= document.getElementById("employee-upload-file").files[0];
	if(employeeExcel == null || employeeExcel == ""){
		alert("尚未選擇人員excel")
		return
	}
	formData.append("employeeExcel",employeeExcel)
	$.ajax({
        method:"POST",
        url:"/Home/ImportEmployeeExcel",
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


