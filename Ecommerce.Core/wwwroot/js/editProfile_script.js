$(document).ready(function () {
    $(".Editable-content").prop("disabled", true);

    $(document).on("click", "#EditContent-btn", function (e) {
        e.preventDefault();
        $(".Editable-content").prop("disabled", function (_, val) {
            return !val;
        });
    });

    $(document).on('submit', '#EditProfileForm', function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        console.log('Submitting edit user form:', formData);

        $.ajax({
            url: '/Dashboard/EditUser',
            type: 'PUT',
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message, 'Success', { timeOut: 6000 });
                } else {
                    toastr.error(response.message, 'Error', { timeOut: 6000 });
                }
            },
            error: function (xhr, status, error) {
                console.error('Error editing user:', error);
                toastr.error('Failed to edit user.', 'Error', { timeOut: 6000 });
            }
        });

    })

});