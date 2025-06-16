$(document).ready(function () {
    
    function GetCountries() {
        $.ajax({
            url: '/Home/GetCountries',
            type: 'GET',
            success: function (data) {
                $("#countrydropdown").html(data);
            },
            error: function (error) {
                console.error('Error in country details:', error);
            }
        });
    }

    $(document).on('click', '#countrydropdown', function () {
        var countryId = $(this).val();
        console.log(countryId, "______");
        $('#statedropdown').html('<option value="">Select State</option>');
        $('#citydropdown').html('<option value="">Select City</option>');

        if (countryId) {
            $.ajax({
                url: '/Home/GetStates',
                type: 'GET',
                data: { countryId: countryId },
                success: function (data) {
                    $("#statedropdown").append(data);
                },
                error: function (error) {
                    console.error('Error in state details:', error);
                }
            });
        }
    });

    $(document).on('click', '#statedropdown', function () {
        var stateId = $(this).val();
        $('#citydropdown').html('<option value="">Select City</option>');
        console.log(stateId, "______state",);
        if (stateId) {
            $.ajax({
                url: '/Home/GetCities',
                type: 'GET',
                data: { stateId: stateId },
                success: function (data) {
                    $("#citydropdown").append(data);
                },
                error: function (error) {
                    console.error('Error in city details:', error);
                }
            });
        }
    });

    GetCountries();
});