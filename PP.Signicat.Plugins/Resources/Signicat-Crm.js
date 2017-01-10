function DocumentSign() {

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

    var params = "entityid=" + entityid + "&entityname=" + entitytypename + "&serverurl=" + serverUrl + "&userid=" + userid + "&username=" + username + "&name=" + name + "&orgname=" + orgName;
    var customParameters = encodeURIComponent(params);
    //Xrm.Utility.openWebResource("pp_documentsign.htm", customParameters, 700, 620);

    var url = "/WebResources/pp_documentsign.htm?Data=" + customParameters;
    var DialogOptions = new Xrm.DialogOptions();
    DialogOptions.width = 850;
    DialogOptions.height = 820;
    Xrm.Internal.openDialog(url, DialogOptions, null, null, CallbackFunction);

}

function CallbackFunction(returnValue) {

}

function OnLoad() {
    var entitynames = GetConfigValue("entitylogicalnames");
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
        url: Xrm.Page.context.getClientUrl() + "/XRMServices/2011/OrganizationData.svc/pp_signicatsettingsSet?$select=pp_value&$filter=pp_name eq '" + value + "'",
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        async: false,
        success: function (data, textStatus, xhr) {
            var results = data.d.results;
            for (var i = 0; i < results.length; i++) {
                var pp_value = results[i].pp_value;
                returnvalue = pp_value;
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Xrm.Page.ui.setFormNotification("Error getting config setting!: " + errorThrown, "ERROR");
        }
    });

    return returnvalue;
}