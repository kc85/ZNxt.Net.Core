$(function () {
    function getAppInstallStatus() {
        $("#loaderDiv").show();
        $.get("./api/appinstaller/status", function (data) {
            console.log(data);
            showRequiredComponent(data.data.status);
        }).fail(function () {
            alert('Error');
        }).always(function () {
            $("#loaderDiv").hide();
        });
    }
    getAppInstallStatus();

    function showRequiredComponent(installStatus) {
        $(".configsection").hide();
        if (installStatus == "DBNotSet") {
            $("#dbconfigdiv").show();
        }
        else if (installStatus == "Init") {
            $("#userconfigdiv").show();
        }
        else if (installStatus == "Finish") {
            $("#installFinish").show(); 
        }
    }
    $("#setDbBtn").click(function () {
        var dbconnectionstring = $("#connString").val();
        var dbname = $("#dbname").val();
        setDB(dbname, dbconnectionstring);
    });
    $("#installBtn").click(function () {
        var email = $("#email").val();
        var passws = $("#pwd").val();
       
        if (email.length == 0 || passws.length == 0) {
            alert("Invalid user name or password");
            return
        }
        $("#installBtn").hide();
        makeAjaxPostCall('./api/appinstaller/install', { 'Email': email, 'Password': passws }, function () {
            getAppInstallStatus();
        }, function () {
            $("#installBtn").show();
            alert("Error in install");
        });

    });
    function setDB(db, connection) {
        $("#setDbBtn").hide();
        makeAjaxPostCall('./api/appinstaller/setdb', { 'ConnectionString': connection, 'Database': db }, function () {
            getAppInstallStatus();
        }, function () {
            $("#setDbBtn").show();
            alert("Error setting up the DB");
        });
    }
    function makeAjaxPostCall(url, data, callback, errorcallback) {
        $("#loaderDiv").show();
        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(data),
            success: function (data) {
                if (data.code == 1) {
                    if (callback != undefined) {
                        callback(data)
                    }
                }
                else {
                    $("#loaderDiv").hide();
                    if (errorcallback != undefined) {
                        errorcallback(data);
                    }
                }
            },
            error: function (jqXHR, exception) {
                $("#loaderDiv").hide();
                if (errorcallback != undefined) {
                    errorcallback(data);
                }
            },
            contentType: "application/json",
            dataType: 'json'
        });
    }
});
