$(function () {
  JSManager = AppJS.getInstance();
  JSManager.bindPartialViewEvent();
  JSManager.setMandatorySymbol();
  $('[data-bs-toggle="tooltip"]').tooltip();
});

function ajaxBegin() {
  JSManager.blockUI("app-body");
}

function ajaxComplete(response) {
  JSManager.unblockUI("app-body");
}

function ajaxSuccess(response) {
  if (response.status === 2000) {
    debugger;
    if (response.child_action) {
      JSManager.reloadChildGrid();
    } else {
      JSManager.reloadGrid();
    }
    JSManager.closeOffCanvas();
    if (JSManager.hasValue(response.message))
      JSManager.showSuccess(response.message);
  } else if (response.status === 2001) {
    let errorHtml = "";
    $.each(response.errors, function (index, item) {
      errorHtml += "<li>" + item.Message + "</li>";
    });

    $(".validation-errors").html(
      '<div class="alert alert-danger alert-dismissible d-flex align-items-center alert-validation-summary" role="alert"><i class="bx bx-xs bx-alarm-exclamation me-2"></i>' +
        errorHtml +
        '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>'
    );
  } else if (response.status === 2002) {
    JSManager.showError(response.message);
  } else {
    JSManager.closeOffCanvas();
    JSManager.showAppError(response.message);
  }
}

function ajaxFailure(response) {
  JSManager.closeOffCanvas();
  JSManager.showAppError(response.responseJSON.message);
}
