function HandleTranslation(lcid) {
    $.translate.code_index = {
        'en': 0,
        'nb': 1
    };

    $.translate.add_codes({
        'boolyes': ['Yes', 'Ja'],
        'boolyno': ['No', 'Nei'],
        'lblselectsignmetod': ['Signing method', 'Signeringsmetode'],
        'auth': ['Authentication?', 'Autentisering?'],
        'lbldaystolive': ['Expiration', 'Frist'],
        'lblsendsms': ['Send SMS?', 'Send SMS?'],
        'lblnotify': ['Notification?', 'Varsel?'],
        'lblsaveorg': ['Save original?', 'Lagre original?'],
        'lblsendcopy': ['Send copy?', 'Send kopi?'],
        'lblsaveinsp': ['Save in Sharepoint?', 'Lagre på SharePoint?'],
        'lblsubject': ['Subject', 'Emne'],
        'subject': ['Email header', 'Epost emne'],
        'lblmessage': ['Message', 'Melding'],
        'message': ['Email message', 'Epost text'],
        'lblselectfiles': ['Select file(s)', 'Velg fil(er)'],
        'fileHelp': ['Plase attach file(s) you want to send for signing.', 'Vennligst legg ved fil(er) du vil sende for signering'],
        'lblsavemerged': ['Save only merged?', 'Lagre kun sammenslått?'],
        'lblrecipeints': ['Recipients', 'Mottakere'],
        'spansearch': ['Search', 'Søk'],
        'spanaccounts': ['Accounts', 'Kunder'],
        'spancontacts': ['Contacts', 'Kontakter'],
        'search': ['Search in contacts or accounts...', 'Søk på kontakter eller kunder...'],
        'thname': ['Name', 'Navn'],
        'thcompany': ['Company', 'Firma'],
        'themail': ['Email', 'Epost'],
        'thmobile': ['Mobile', 'Mobil'],
        'btnCancel': ['Cancel', 'Avbryt'],
        'btnDone': ['Done', 'OK'],

        //Tooltip translations
        'lblselectsignmetodtitle': ['Chose a method for signing the documents.', 'Velg en signerings metode.'],
        'lblisink': ['The signing will include the Ink method in addition.', 'Signeringen vil inkludere Ink metoden i tillegg.'],
        'authtitle': ['The user will have to authenticate before seeing the document.', 'Mottakeren må autentisere seg før han kan se på dokumentet.'],
        'lbldaystolivetitle': ['How long will the documents be available for signing.', 'Hvor lenge skal dokumentet være tilgjengelig for signering'],
        'lblsendsmstitle': ['Send the link for signing in SMS to the reciever.', 'Send linken for signering via SMS til mottaker.'],
        'lblnotifytitle': ['An email will be sent to you when a document is signed.', 'Du får en epost når en mottaker har signert et dokument.'],
        'lblsaveorgtitle': ['The uploaded documents will be saved in CRM notes.', 'Det vedlagtet dokumentet vil bli lagret på notat feltet i CRM.'],
        'lblsendcopytitle': ['The signed documents will be sent as a copy to the signatories.', 'En kopi av det signerte dokumentet blir sendt til mottaker(e) på epost.'],
        'lblsaveinsptitle': ['The signed documents will be saved in the Sharepoint Document Location of the parent record.', 'Det signerte dokumentet blir lagret på SharePoint dokumentlokasjonen til morobjektet.'],
        'lblselectfilestitle': ['Chose a file from your local computer.', 'Velg en fil lokalt fra din PC.'],
        'lblsavemergedtitle': ['If NO, then all the signed documents inclusive the merged ones will be saved in CRM notes.', 'Hvis NEI, blir alle de signerte dokumentene, inklusiv de sammenslåtte, bli lagret på CRM notat feltet.'],
        'lblrecipeintstitle': ['These contacts will receive an email for signing the documents.', 'Disse kontaktene vil motta en epost for signering av dokumentet.'],

        //Error & info translations
        'doclocnotify': ['This record has no Sharepoint location.', 'Dene oppføringen har ingen SharePoint lokasjon.'],
        'searchnotify': ['Please chose a search entity!', 'Vennligst velg et entitet å søke på!'],
        'noresultnotify': ['No search results!', 'Ingen søke resultater!'],
        'emptyemailnotify': ['The customer must have an email address.', 'Mottakeren må ha en epost adresse.'],
        'customeraddednotify': ['A customer with the same email is allready added!', 'En mottaker med samme epost adresse er allrede lagt til!'],
        'emptysubjectnotify': ['You must write a subject!', 'Du fylle må ut emne!'],
        'emptymessagenotify': ['You must write a message!', 'Du må fylle ut meldingen!'],
        'onlypdfnotify': ['Only PDF is allowed!', 'Kun PDF filer er tillatt!'],
        'addrecipientsnotify': ['You must add recipients!', 'Du må legge til mottakere!'],
        'requestsentnotify': ['Request is sent to Signicat!', 'Request er sendt til Signicat!'],
        'errorconnnotify': ['Error connecting to Signicat!', 'Feil ved tilkobling til Signicat!'],
        'nofilenotify': ['You must chose a document!', 'Do må velge mist ett dokument!'],

        //SIGNICAT API file
        'errordocloc': ['Error finding documentlocation: ', 'Feil ved dokumentlokasjon: '],
        'orginialdoc': ['Original document ', 'Original dokument '],
        'uploadedby': ['The document is uploaded by ', 'Dokumentet har blitt lastet opp av '],
        'notcreatednotify': ['The document signing is not created, error: ', 'Dokumentsigneringen har ikke blitt opprettet, feil: '],
        'emailsendnotify': ['Could not send email, error: ', 'Kunne ikke sende epost, feil: '],
        'errorsignicatresultnotify': ['Error creating Signicat Result, error: ', 'Feil ved opprettelse av Signicat resultat, feil: '],
        'errorassocnotify': ['Assosiation failed! Error: ', 'Asosieringen feilet! feil: '],
        'errornotecreatenotify': ['Error creating the note!: ', 'Feil ved opprettelse av notat!: '],
        'errorfindusernotify': ['Cannot find user!: ', 'Kan ikke finne bruker!: '],
        'errorparampassnotify': ['No data parameter was passed to this page.', 'Ingen dataparametre har blitt sendt til denne websiden.'],
        'errorfindconfig': ['Error getting config value!: ', 'Feil ved opphenting av setting verdi!: ']
        

        

    });

    if (lcid == "1044")
        $.translate.set_language('nb');

    if (lcid == "1033")
        $.translate.set_language('en');


    //var yesreplaced9 = $('#lblradios-9').html().replace("Yes", $.translate.get_text('boolyes'));
    //var noreplaced10 = $('#lblradios-10').html().replace("No", $.translate.get_text('boolyno'));
    //var yesreplaced0 = $('#lblradios-0').html().replace("Yes", $.translate.get_text('boolyes'));
    //var yesreplaced2 = $('#lblradios-2').html().replace("Yes", $.translate.get_text('boolyes'));
    //var noreplaced3 = $('#lblradios-3').html().replace("No", $.translate.get_text('boolyno'));
    //var yesreplaced11 = $('#lblradios-11').html().replace("Yes", $.translate.get_text('boolyes'));
    //var noreplaced12 = $('#lblradios-12').html().replace("No", $.translate.get_text('boolyno'));
    //var yesreplaced4 = $('#lblradios-4').html().replace("Yes", $.translate.get_text('boolyes'));
    //var noreplaced5 = $('#lblradios-5').html().replace("No", $.translate.get_text('boolyno'));
    //var yesreplaced6 = $('#lblradios-6').html().replace("Yes", $.translate.get_text('boolyes'));
    //var noreplaced7 = $('#lblradios-7').html().replace("No", $.translate.get_text('boolyno'));
    //var yesreplaced13 = $('#lblradios-13').html().replace("Yes", $.translate.get_text('boolyes'));
    //var noreplaced14 = $('#lblradios-14').html().replace("No", $.translate.get_text('boolyno'));
    //var yesreplaced15 = $('#lblradios-15').html().replace("Yes", $.translate.get_text('boolyes'));
    //var noreplaced16 = $('#lblradios-16').html().replace("No", $.translate.get_text('boolyno'));

    //var str = $('#lblradios-1').html();
    //var pos = str.lastIndexOf('No');
    //var noreplaced1 = str.substring(0, pos) + $.translate.get_text('boolyno') + str.substring(pos + 2);

    //$('#lblradios-9').html(yesreplaced9);
    //$('#lblradios-10').html(noreplaced10);
    //$('#lblradios-0').html(yesreplaced0);
    //$('#lblradios-1').html(noreplaced1);
    //$('#lblradios-2').html(yesreplaced2);
    //$('#lblradios-3').html(noreplaced3);
    //$('#lblradios-11').html(yesreplaced11);
    //$('#lblradios-12').html(noreplaced12);
    //$('#lblradios-4').html(yesreplaced4);
    //$('#lblradios-5').html(noreplaced5);
    //$('#lblradios-6').html(yesreplaced6);
    //$('#lblradios-7').html(noreplaced7);
    //$('#lblradios-13').html(yesreplaced13);
    //$('#lblradios-14').html(noreplaced14);
    //$('#lblradios-15').html(yesreplaced15);
    //$('#lblradios-16').html(noreplaced16);

    $('#lblselectsignmetod').text($.translate.get_text('lblselectsignmetod'));
    $('#lblauth div h5').text($.translate.get_text('auth'));
    $('#lbldaystolive').text($.translate.get_text('lbldaystolive'));
    $('#lblsendsms div h5').html($.translate.get_text('lblsendsms'));
    $('#lblnotify div h5').html($.translate.get_text('lblnotify'));
    $('#lblsaveorg div h5').html($.translate.get_text('lblsaveorg'));
    $('#lblsendcopy div h5').html($.translate.get_text('lblsendcopy'));
    $('#lblsaveinsp div h5').html($.translate.get_text('lblsaveinsp'));
    $('#lblsubject').html($.translate.get_text('lblsubject'));
    $('#subject').attr("placeholder", $.translate.get_text('subject'));
    $('#lblmessage').html($.translate.get_text('lblmessage'));
    $('#message').attr("placeholder", $.translate.get_text('message'));
    $('#lblselectfiles').html($.translate.get_text('lblselectfiles'));
    $('#fileHelp').html($.translate.get_text('fileHelp'));
    $('#lblsavemerged div h5').html($.translate.get_text('lblsavemerged'));
    $('#lblrecipeints').html($.translate.get_text('lblrecipeints'));
    $('#spansearch').html($.translate.get_text('spansearch'));
    $('#spanaccounts').html($.translate.get_text('spanaccounts'));
    $('#spancontacts').html($.translate.get_text('spancontacts'));
    $('#search').attr("placeholder", $.translate.get_text('search'));
    $('#thname').html($.translate.get_text('thname'));
    $('#thcompany').html($.translate.get_text('thcompany'));
    $('#thmobile').html($.translate.get_text('thmobile'));
    $('#themail').html($.translate.get_text('themail'));
    $('#btnCancel').html($.translate.get_text('btnCancel'));
    $('#btnDone').html($.translate.get_text('btnDone'));

    //Tooltip translations
    $('#lblselectsignmetod').attr('title', $.translate.get_text('lblselectsignmetodtitle'));
    $('#lblink').attr('title', $.translate.get_text('lblisink'));
    $('#lblauth').attr('title', $.translate.get_text('authtitle'));
    $('#lbldaystolive').attr('title', $.translate.get_text('lbldaystolivetitle'));
    $('#lblsendsms').attr('title', $.translate.get_text('lblsendsmstitle'));
    $('#lblnotify').attr('title', $.translate.get_text('lblnotifytitle'));
    $('#lblsaveorg').attr('title', $.translate.get_text('lblsaveorgtitle'));
    $('#lblsendcopy').attr('title', $.translate.get_text('lblsendcopytitle'));
    $('#lblsaveinsp').attr('title', $.translate.get_text('lblsaveinsptitle'));
    $('#lblselectfiles').attr('title', $.translate.get_text('lblselectfilestitle'));
    $('#lblsavemerged').attr('title', $.translate.get_text('lblsavemergedtitle'));
    $('#lblrecipeints').attr('title', $.translate.get_text('lblrecipeintstitle'));
}