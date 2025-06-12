
$(document).ready(function () {
    let count = false;
    let count2 = false;
    let count3 = false;
    
    // function for toggle password visibility
    function togglePassword(eyeOpen, eyeClose, input, countState) {
        countState = !countState;
        if (countState) {
            $(eyeOpen).addClass("active").removeClass("inactive");
            $(eyeClose).removeClass("active").addClass("inactive");
            $(input).attr("type", "text");
        } else {
            $(eyeOpen).addClass("inactive").removeClass("active");
            $(eyeClose).removeClass("inactive").addClass("active");
            $(input).attr("type", "password");
        }

        return countState;
    }

    // Toggle password visibility on click (at index page for login)
    $(".openeye, .closeeye").on("click", function () {
        count = togglePassword(".openeye", ".closeeye", "#exampleInputPassword", count);
    });

    // Toggle password visibility on click (at reset password page for password)
    $(".openeye1, .closeeye1").on("click", function () {
        count2 = togglePassword(".openeye1", ".closeeye1", "#exampleInputPassword2", count2);
    });

    // Toggle password visibility on click (at reset password page for confirm password)
    $(".openeye2, .closeeye2").on("click", function () {
        count3 = togglePassword(".openeye2", ".closeeye2", "#exampleInputPassword3", count3);
    });
    

    // logout modal
    var LogoutModal = new bootstrap.Modal(document.getElementById('LogoutModal'), {
        keyboard: false,
        backdrop: 'static'
    });

    // handle logout button click
    $("#LogoutButton").on("click", function (e) {
        e.preventDefault();
        LogoutModal.show();
    });


});
