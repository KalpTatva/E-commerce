$(document).ready(function () {
    toastr.options.closeButton = true;
    var errorMessage = window.errorMessage;
    if (errorMessage) {
        toastr.error(errorMessage, 'Error', { setTimeout: 6000 });
    }

    var successMessage = window.successMessage;
    if (successMessage) {
        toastr.success(successMessage, 'success', { timeOut: 6000 });
    }
});