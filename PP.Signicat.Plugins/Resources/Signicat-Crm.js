var userLCid;

function DocumentSign() {

    userLCid = Xrm.Page.context.getUserLcid();
    var entityid = Xrm.Page.data.entity.getId();
    var entitytypename = Xrm.Page.data.entity.getEntityName();
    var serverUrl = Xrm.Page.context.getClientUrl();
    var userid = Xrm.Page.context.getUserId();
    var username = Xrm.Page.context.getUserName();
    var name = "";
    if (entitytypename == "incident" || entitytypename == "contract")
        name = Xrm.Page.getAttribute("title").getValue();
    else {
        var namevalue = Xrm.Page.getAttribute("name");
        if (namevalue != null)
            name = namevalue.getValue();
    }

    var orgName = Xrm.Page.context.getOrgUniqueName();

    var params = "entityid=" + entityid + "&entityname=" + entitytypename + "&serverurl="
        + serverUrl + "&userid=" + userid + "&username=" + username + "&name=" + name
        + "&orgname=" + orgName + "&lcid=" + userLCid;
    var customParameters = encodeURIComponent(params);
    //Xrm.Utility.openWebResource("pp_documentsign.htm", customParameters, 700, 620);

    var url = "/WebResources/pp_documentsign.htm?Data=" + customParameters;
    var DialogOptions = new Xrm.DialogOptions();
    DialogOptions.width = 950;
    DialogOptions.height = 820;
    Xrm.Internal.openDialog(url, DialogOptions, null, null, CallbackFunction);

}

function CallbackFunction(returnValue) {

}

function OnLoad() {
    var entitynames = GetConfigValue("pp_entitylogicalnames");
    var entitynameArray = entitynames.split(',');

    $.each(entitynameArray, function (index, value) {
        HideEmptyLookup(value);
    });
}

function HideEmptyLookup(lookupname) {
    var lookupControl = Xrm.Page.getAttribute(lookupname).getValue();
    if (lookupControl == null)
        Xrm.Page.getControl(lookupname).setVisible(false);
}

function GetConfigValue(value) {
    var returnvalue;

    $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: Xrm.Page.context.getClientUrl() + "/XRMServices/2011/OrganizationData.svc/pp_signicatconfigSet?$select=pp_entitylogicalnames,pp_webapiurl",
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        async: false,
        success: function (data, textStatus, xhr) {
            var results = data.d.results;
            if (results.length == 1) {

                var pp_entitylogicalnames = results[0].pp_entitylogicalnames;
                var pp_webapiurl = results[0].pp_webapiurl;
            
                if (value == "pp_entitylogicalnames")
                    returnvalue = pp_entitylogicalnames;

                if (value == "pp_webapiurl")
                    returnvalue = pp_webapiurl;

            }
        },
        error: function (xhr, textStatus, errorThrown) {
            if (userLCid == "1033")
                Xrm.Page.ui.setFormNotification("Error getting config value!: " + errorThrown, "ERROR");
            if (userLCid == "1044")
                Xrm.Page.ui.setFormNotification("Feil ved opphenting av config verdi!: " + errorThrown, "ERROR");
        }
    });

    return returnvalue;
}