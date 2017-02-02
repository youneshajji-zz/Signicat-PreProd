
function CheckDocLocation(entityid) {
    var hasValue = false;
    $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        async: false,
        url: Xrm.Page.context.getClientUrl() + "/XRMServices/2011/OrganizationData.svc/SharePointDocumentLocationSet?$filter=RegardingObjectId/Id eq (guid'" + entityid + "')",
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data, textStatus, xhr) {
            var results = data.d.results;
            //var SharePointDocumentLocationId = results[i].SharePointDocumentLocationId;
            if (results.length > 0)
                hasValue = true;
        },
        error: function (xhr, textStatus, errorThrown) {
            $.notify($.translate.get_text('notcreatednotify') + errorThrown, "error");
        }
    });

    return hasValue;
}

function createRecord(sdsurls, odataSetName, files, customers, saveoriginalfile, saveinsp, saveonlymerged) {
    var obj = getDataParam();
    var entityname = obj.entityname;
    var entityid = obj.entityid;
    var serverUrl = obj.serverurl;
    var userid = obj.userid;
    var username = obj.username;

    var signingmetod = $("#selectsignmetod").val();
    //var name = $("#name").val();
    //var email = $("#email").val();
    var subject = $("#subject").val();
    var message = $("#message").val();
    var radiovalueAuth = $("#radiosAuth input[type='radio']:checked").val();
    var radiovalueSendCopy = $("#radiosSendCopy input[type='radio']:checked").val();
    //var authmetod = $("#authmetod").val();
    var daystolive = $("#daystolive").val();
    var requestid = sdsurls[0].split('&').shift().split('=').pop();

    var authmetod;
    if (radiovalueAuth == 9) {
        if (signingmetod == 1) //BankID
            authmetod = 1;
        if (signingmetod == 2)//Sms/E-mail
            authmetod = 2;
        if (signingmetod == 3)//Social
            authmetod = 3;
    }
    else if (radiovalueAuth == 10)
        authmetod = 0;

    var sendcopy;
    if (radiovalueSendCopy == 11)
        sendcopy = true;
    else if (radiovalueSendCopy == 12)
        sendcopy = false;

    var entity = {};
    entity.pp_name = subject;
    entity.pp_signing = { Value: signingmetod };
    entity.pp_subject = subject;
    entity.pp_message = message;
    //entity.EmailAddress = email;
    entity.statuscode = { Value: 778380000 };
    entity.pp_authentication = { Value: authmetod };
    entity.pp_saveindocumentlocation = saveinsp;
    entity.pp_saveonlymerged = saveonlymerged;
    entity.pp_requestid = requestid;
    entity.pp_sendcopy = sendcopy;

    if (daystolive != "")
        entity.pp_daystolive = daystolive;

    //var entitynames = GetConfigValue("entitylogicalnames");
    //var entitynameArray = entitynames.split(',');

    //$.each(entitynameArray, function (index, value) {
    //    if (value.indexOf(entityname) >= 0) {
    //        alert(value);
    //        alert(entityname);
    //        entity.value = {
    //            Id: entityid,
    //            LogicalName: entityname
    //        }
    //    }
    //});

    if (entityname == "quote")
        entity.pp_quoteid = {
            Id: entityid,
            LogicalName: entityname
        }
    if (entityname == "account")
        entity.pp_accountid = {
            Id: entityid,
            LogicalName: entityname
        }
    if (entityname == "salesorder")
        entity.pp_salesorderid = {
            Id: entityid,
            LogicalName: entityname
        }
    if (entityname == "opportunity")
        entity.pp_opportunityid = {
            Id: entityid,
            LogicalName: entityname
        }
    if (entityname == "incident")
        entity.pp_incidentid = {
            Id: entityid,
            LogicalName: entityname
        }
    if (entityname == "contract")
        entity.pp_contractid = {
            Id: entityid,
            LogicalName: entityname
        }

    //Parse the entity object into JSON
    var jsonEntity = window.JSON.stringify(entity);

    //The OData end-point
    var ODATA_ENDPOINT = "/XRMServices/2011/OrganizationData.svc";
    //Asynchronous AJAX function to Create a CRM record using OData
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: serverUrl + ODATA_ENDPOINT + "/" + odataSetName,
        data: jsonEntity,
        async: false,
        beforeSend: function (XMLHttpRequest) {
            //Specifying this header ensures that the results will be returned as JSON.
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data, textStatus, XmlHttpRequest) {
            //$.notify('Documentsigning is created!', "success");
            var docsignid = data.d.pp_documentsigningId;
            AssociateCustomers(docsignid, customers, serverUrl);
            //$.notify('Contact(s) are added!', "success");

            for (var i = 0; i < sdsurls.length; i++) {
                CreateSignicatUrl(docsignid, sdsurls[i], serverUrl, customers[i]);
            }

            //$.notify('Signicat Task(s) are created!', "success");

            if (saveoriginalfile) {
                for (var i = 0; i < files.length; i++) {
                    (function (file) {
                        AddNotes(serverUrl, $.translate.get_text('orginialdoc') + (i + 1), $.translate.get_text('uploadedby') + username, docsignid, "pp_documentsigning", files[i], "application/pdf");
                    })(files[i]);
                }
            }

            SendEmails(docsignid, serverUrl);
        },
        error: function (XmlHttpRequest, textStatus, errorThrown) {
            $.notify($.translate.get_text('notcreatednotify') + errorThrown, "error");
        }
    });
}

function SendEmails(documentsigningid, serverUrl) {
    var entity = {};
    entity.pp_sendemail = true;

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        async: false,
        url: serverUrl + "/XRMServices/2011/OrganizationData.svc/pp_documentsigningSet(guid'" + documentsigningid + "')",
        data: JSON.stringify(entity),
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
            XMLHttpRequest.setRequestHeader("X-HTTP-Method", "MERGE");
        },
        success: function (data, textStatus, xhr) {
            //$.notify('Mails are being sent now!', "success");
        },
        error: function (xhr, textStatus, errorThrown) {
            $.notify($.translate.get_text('emailsendnotify') + errorThrown, "error");
        }
    });
}

function CreateSignicatUrl(docsignid, sdsurl, serverUrl, customer) {
    var parts = sdsurl.split('=');
    var taskid = parts.pop();

    var entity = {};
    entity.pp_documentsigningid = {
        Id: docsignid,
        LogicalName: "pp_documentsigning"
    };

    if (customer.entityLogicalName == "contact")
        entity.pp_customerid = {
            Id: customer.id,
            LogicalName: customer.entityLogicalName
        };


    if (customer.entityLogicalName == "account")
        entity.pp_customerid = {
            Id: customer.id,
            LogicalName: customer.entityLogicalName
        };

    entity.pp_sdsurl = sdsurl;
    entity.pp_name = taskid;
    entity.statuscode = { Value: 1 };

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: serverUrl + "/XRMServices/2011/OrganizationData.svc/pp_signicatdocurlSet",
        data: JSON.stringify(entity),
        async: false,
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data, textStatus, xhr) {
            //$.notify('Signicat Task is created!', "success");
            var result = data.d;
            var newEntityId = result.pp_signicatdocurlId;
            return newEntityId;
        },
        error: function (xhr, textStatus, errorThrown) {
            $.notify($.translate.get_text('errorsignicatresultnotify') + errorThrown, "error");
            $('#loading-indicator').hide();
        }
    });
}

function AssociateCustomers(documentsigningid, customers, serverUrl) {
    for (var i = 0; i < customers.length; i++) {
        if (customers[i].id != "") {
            var association = {};
            var url;
            if (customers[i].entityLogicalName == "account") {
                association.uri = serverUrl + "/XRMServices/2011/OrganizationData.svc/AccountSet(guid'" + customers[i].id + "')";
                url = "/XRMServices/2011/OrganizationData.svc/pp_documentsigningSet(guid'" + documentsigningid + "')/$links/pp_pp_documentsigning_account";
            }

            if (customers[i].entityLogicalName == "contact") {
                association.uri = serverUrl + "/XRMServices/2011/OrganizationData.svc/ContactSet(guid'" + customers[i].id + "')";
                url = "/XRMServices/2011/OrganizationData.svc/pp_documentsigningSet(guid'" + documentsigningid + "')/$links/pp_pp_documentsigning_contact";
            }

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: serverUrl + url,
                data: JSON.stringify(association),
                async: false,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                success: function (data, textStatus, xhr) {
                    //$.notify('Contact is added!', "success");
                },
                error: function (xhr, textStatus, errorThrown) {
                    $.notify($.translate.get_text('errorassocnotify') + errorThrown, "error");
                    $('#loading-indicator').hide();
                }
            });
        }
    }
}
function AddNotes(serverUrl, noteSubject, noteText, entityid, entityname, file, mType) {
    var fileReader = new FileReader();
    fileReader.onloadend = function (event) {
        var fileData = event.target.result;
        fileData = fileData.substring(fileData.indexOf(";base64,") + 8, fileData.length);

        var fName = file.name;
        var noteText = noteText;
        var ODATA_ENDPOINT = "/XRMServices/2011/OrganizationData.svc";
        var objAnnotation = new Object();
        var ODATA_EntityCollection = "/AnnotationSet";
        objAnnotation.NoteText = noteText;
        objAnnotation.Subject = noteSubject;

        objAnnotation.DocumentBody = fileData;
        objAnnotation.FileName = fName;
        objAnnotation.MimeType = mType;

        var refEntity = new Object();
        refEntity.LogicalName = entityname;
        refEntity.Id = entityid;

        objAnnotation.ObjectId = refEntity;
        objAnnotation.ObjectTypeCode = refEntity.LogicalName;

        // Parse the entity object into JSON
        var jsonEntity = window.JSON.stringify(objAnnotation);

        // Asynchronous AJAX function to Create a CRM record using OData
        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: serverUrl + ODATA_ENDPOINT + ODATA_EntityCollection,
            data: jsonEntity,
            async: false,
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data, textStatus, XmlHttpRequest) {
                //$.notify("Note with the original document is created: " + file.name, "info");
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                $.notify($.translate.get_text('errornotecreatenotify') + errorThrown, "error");
                $('#loading-indicator').hide();
            }
        });
    };
    fileReader.onerror = function (event) {
        $.notify($.translate.get_text('erroruploadnotify') + event.target.error.name, "error");
        $('#loading-indicator').hide();
    };
    // begin the read operation
    fileReader.readAsDataURL(file);
}

function GetUserMail(userid) {
    var email = "";

    $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        async: false,
        url: Xrm.Page.context.getClientUrl() + "/XRMServices/2011/OrganizationData.svc/SystemUserSet(guid'" + userid + "')?$select=InternalEMailAddress",
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data, textStatus, xhr) {
            var result = data.d;
            var internalEMailAddress = result.InternalEMailAddress;
            email = internalEMailAddress;
        },
        error: function (xhr, textStatus, errorThrown) {
            $.notify($.translate.get_text('errorfindusernotify') + errorThrown, "error");
            $('#loading-indicator').hide();
        }
    });
    return email;
}

function getDataParam() {
    //Get the any query string parameters and load them
    //into the vals array

    var vals = new Array();
    if (location.search != "") {
        vals = location.search.substr(1).split("&");
        for (var i in vals) {
            vals[i] = vals[i].replace(/\+/g, " ").split("=");
        }
        //look for the parameter named 'data'
        var found = false;
        for (var i in vals) {
            if (vals[i][0].toLowerCase() == "data") {
                var obj = parseDataValue(vals[i][1]);
                found = true;
                return obj;
                break;
            }
        }
        if (!found)
        { noParams(); }
    }
    else {
        noParams();
    }
}

function parseDataValue(datavalue) {
    if (datavalue != "") {
        var vals = new Array();
        vals = decodeURIComponent(datavalue).split("&");

        for (var i in vals) {
            vals[i] = vals[i].replace(/\+/g, " ").split("=");
        }

        var obj = new Object();
        obj.entityid = vals[0][1];
        obj.entityname = vals[1][1];
        obj.serverurl = vals[2][1];
        obj.userid = vals[3][1];
        obj.username = vals[4][1];
        obj.name = vals[5][1];
        obj.orgname = vals[6][1];
        obj.lcid = vals[7][1];

        return obj;
    }
    else {
        noParams();
    }
}

function noParams() {
    var message = document.createElement("p");
    setText(message, $.translate.get_text('errorparampassnotify'));

    document.body.appendChild(message);
}
//Added for cross browser support.
function setText(element, text) {
    if (typeof element.innerText != "undefined") {
        element.innerText = text;
    }
    else {
        element.textContent = text;
    }
}
