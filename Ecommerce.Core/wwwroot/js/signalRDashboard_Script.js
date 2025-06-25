$(document).ready(function () { 

    var $bell = $('#notificationBell');
    var $popup = $('#notificationPopup');
    
    // hub connection 
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // start connection
    connection.start()
        .then(() => console.log("SignalR Connected"))
        .catch(err => console.error(err.toString()));

    // receive notification
    connection.on("ReceiveNotification", function (message) {
        // fetch function for getting notification count
        FetchNoficationCount();
        // fetch function for getting notifications of user
        FetchNotifications();
    });

    // function to fetch notification count
    function FetchNoficationCount() {
        $.ajax({
            url: "/Dashboard/GetNotificationCount",
            type: "GET",
            success: function (data) {
                if (data.success) {
                    $("#notificationCount").text(data.count);
                    if (data.count > 0) {
                        $("#notificationCount").show();
                    } else {
                        $("#notificationCount").hide();
                    }
                }
            },
            error: function (error) {
                console.error("Error fetching notification count:", error);
            }
        });
    }

    function FetchNotifications() {
        $.ajax({
            url: "/Dashboard/GetNotifications",
            type: "GET",
            success: function (data) {
                if (data.success) {
                    var notificationList = $("#notificationList");
                    notificationList.empty(); // Clear existing notifications

                    if (data.notifications && data.notifications.length > 0) {
                        data.notifications.forEach(function (notification) {
                            let date = new Date(notification.createdAt);
                            let formatted = date.toLocaleString();

                            var listItem = $("<li>")
                                .addClass("notification-item")
                                .html(
                                    "<span>" + notification.notification1 + "</span><br>" +
                                    "<small class='text-muted'>" + formatted + "</small>"
                                );
                            notificationList.append(listItem);
                        });
                    } else {
                        notificationList.append(
                            $("<li>")
                                .addClass("text-center text-muted")
                                .text("No new notifications.")
                        );
                    }
                }
            },
            error: function (error) {
                console.error("Error fetching notifications:", error);
            }
        });
    }

    // Position popup relative to bell
    function positionPopup() {
        // Adjust logic below as per your layout
        var bellOffset = $bell.offset();
        var bellHeight = $bell.outerHeight();
        var popupWidth = $popup.outerWidth();
        var left = bellOffset.left + $bell.outerWidth() - popupWidth;

        $popup.css({
            position: 'absolute',
            top: (bellOffset.top + bellHeight + 8) + 'px',
            left: left + 'px',
            zIndex: 9999
        });
    }

    // Show popup
    function showNotificationPopup() {
        $popup.show();
        positionPopup();
        setTimeout(function () { $popup.focus(); }, 10);
    }

    // Hide popup
    function closeNotificationPopup() {
        $popup.hide();
    }

    // Toggle popup on bell click
    $bell.on('click', function (e) {
        e.stopPropagation();
        if ($popup.is(':visible')) {
            closeNotificationPopup();
        } else {
            showNotificationPopup();
        }
    });

    // Hide popup when clicking outside
    $(document).on('click', function (e) {
        if (
            !$popup.is(e.target) &&
            $popup.has(e.target).length === 0 &&
            !$bell.is(e.target) &&
            $bell.has(e.target).length === 0
        ) {
            closeNotificationPopup();
        }
    });


    $(document).on('click','.ReadAllBtn', function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Dashboard/MarkAllNotificationsAsRead",
            type: "POST",
            success: function (data) {
                if (data.success) {
                    var notificationList = $("#notificationList");
                    notificationList.empty();
                    notificationList.append(
                        $("<li>")
                            .addClass("text-center text-muted")
                            .text("No new notifications.")
                    );
                    FetchNotifications(); // Refresh notifications
                    FetchNoficationCount(); // Refresh notification count
                }
            },
            error: function (error) {
                console.error("Error marking notifications as read:", error);
            }
        });
    })

    // Initial fetches
    FetchNotifications();
    FetchNoficationCount();
});