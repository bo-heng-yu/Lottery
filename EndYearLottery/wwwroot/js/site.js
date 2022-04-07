
$( document ).ready(function() {
    $("#decision-modal,#alert-modal,#success-modal,#fail-modal").on("hidden.bs.modal", function (e) {
        var checkModal = false;
        $(".modal").each(function(){
            if($(this).hasClass("show")){
                checkModal = true;
                return;
            }
        })
        if(checkModal){
            $("body").addClass("modal-open");
        }
    });

});
function decisionMessage(msg,funcAccessStr){
    $("#decision-modal-msg").html(msg);
    $("#decision-modal-access").unbind("click");
    $("#decision-modal-access").bind("click",function(){
        eval(funcAccessStr);
        $("#decision-modal").modal("hide");
    });
    $("#decision-modal").modal({
        backdrop: "static"
    });
}
function successMesssage(msg){
    $("#success-modal-msg").html(msg);
    $("#success-modal").modal("show");
}
function failMesssage(msg){
    $("#fail-modal-msg").html(msg);
    $("#fail-modal").modal("show");
}
function alertMesssage(msg){
    $("#alert-modal-msg").html(msg);
    $("#alert-modal").modal("show");
}