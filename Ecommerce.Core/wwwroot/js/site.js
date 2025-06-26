$(document).ready(function () {
    let count = false;
    let count2 = false;
    let count3 = false;


    // logout modal
    var LogoutModal = new bootstrap.Modal(document.getElementById('LogoutModal'), {
        keyboard: false,
        backdrop: 'static'
    });

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

    // handle logout button click
    $("#LogoutButton").on("click", function (e) {
        e.preventDefault();
        LogoutModal.show();
    });

    

    // js for side bar

    var $asidebar = $(".sidebar-dropdown");
    var $asidebarToggle = $(".E-commerce");

    $(document).on("click", function (e) {
        if (!$asidebar.is(e.target) && $asidebar.has(e.target).length === 0 && !$asidebarToggle.is(e.target)) {
            $asidebar.hide();
        }
    });

    $asidebarToggle.on("click", function (e) {
        e.preventDefault();
        e.stopPropagation();
        if($asidebar.is(":visible")) {
            $asidebar.hide();
        }else{
            $asidebar.show();
        }
    });




});
