function RemoveAuthorFromFieldSet(index) {
    var currentIndex = parseInt(document.getElementById('authorsIndex').value);
    if (currentIndex == 0)
        return;

    //move values
    for (var i = index; i < currentIndex; i++) {
        $('#Authors_' + i + '__FirstnameRussian').val($('#Authors_' + (i + 1).toString() + '__FirstnameRussian').val());
        $('#Authors_' + i + '__LastnameRussian').val($('#Authors_' + (i + 1).toString() + '__LastnameRussian').val());
        $('#Authors_' + i + '__PatronymRussian').val($('#Authors_' + (i + 1).toString() + '__PatronymRussian').val());
        $('#Authors_' + i + '__FirstnameEnglish').val($('#Authors_' + (i + 1).toString() + '__FirstnameEnglish').val());
        $('#Authors_' + i + '__LastnameEnglish').val($('#Authors_' + (i + 1).toString() + '__LastnameEnglish').val());
        $('#Authors_' + i + '__PatronymEnglish').val($('#Authors_' + (i + 1).toString() + '__PatronymEnglish').val());
        $('#Authors_' + i + '__Email').val($('#Authors_' + (i + 1).toString() + '__Email').val());
        $('#Authors_' + i + '__PersonalWeb').val($('#Authors_' + (i + 1).toString() + '__PersonalWeb').val());
        $('#Authors_' + i + '__PlaceOfWork').val($('#Authors_' + (i + 1).toString() + '__PlaceOfWork').val());
        $('#Authors_' + i + '__SPIN').val($('#Authors_' + (i + 1).toString() + '__SPIN').val());
        $('#Authors_' + i + '__ResearcherID').val($('#Authors_' + (i + 1).toString() + '__ResearcherID').val());
        $('#Authors_' + i + '__ORCID').val($('#Authors_' + (i + 1).toString() + '__ORCID').val());
    }

    //delete last author input group
    $('#authors_' + currentIndex.toString()).remove();
    $('#removeBtn_' + currentIndex.toString()).remove();
    $('#skipBlock_' + (currentIndex - 1).toString()).remove();
    $('#addBtn_' + currentIndex.toString()).attr('id', 'addBtn_' + (currentIndex - 1).toString());
    $('#skipBlock_' + (currentIndex).toString()).attr('id', 'skipBlock_' + (currentIndex - 1).toString());
    if (currentIndex - 1 == 0) {
        $('#removeBtn_' + (currentIndex - 1).toString()).remove();
    }

    //saving decremented counter
    currentIndex--;
    document.getElementById('authorsIndex').value = currentIndex.toString();
};

function TrimAuthorsFieldSet() {
    var currentIndex = parseInt(document.getElementById('authorsIndex').value);

    //move values
    for (var i = 0; i < currentIndex+1; i++) {
        $('#Authors_' + i + '__FirstnameRussian').val($('#Authors_' + i + '__FirstnameRussian').val().trim());
        $('#Authors_' + i + '__LastnameRussian').val($('#Authors_' + i + '__LastnameRussian').val().trim());
        $('#Authors_' + i + '__PatronymRussian').val($('#Authors_' + i + '__PatronymRussian').val().trim());
        $('#Authors_' + i + '__FirstnameEnglish').val($('#Authors_' + i + '__FirstnameEnglish').val().trim());
        $('#Authors_' + i + '__LastnameEnglish').val($('#Authors_' + i + '__LastnameEnglish').val().trim());
        $('#Authors_' + i + '__PatronymEnglish').val($('#Authors_' + i + '__PatronymEnglish').val().trim());
        $('#Authors_' + i + '__Email').val($('#Authors_' + i + '__Email').val().trim());
        $('#Authors_' + i + '__PersonalWeb').val($('#Authors_' + i + '__PersonalWeb').val().trim());
        $('#Authors_' + i + '__PlaceOfWork').val($('#Authors_' + i + '__PlaceOfWork').val().trim());
        $('#Authors_' + i + '__SPIN').val($('#Authors_' + i + '__SPIN').val().trim());
        $('#Authors_' + i + '__ResearcherID').val($('#Authors_' + i + '__ResearcherID').val().trim());
        $('#Authors_' + i + '__ORCID').val($('#Authors_' + i + '__ORCID').val().trim());
    }
}
function PopulateAuthorsFieldSet() {
    var currentIndex = parseInt(document.getElementById('authorsIndex').value);
    var $newAuthor = $('#authors_' + currentIndex.toString()).clone();
    $newAuthor.attr('id', $newAuthor.attr('id').replace(/\d+$/, function (str) { return parseInt(str) + 1; }));
    //updating attributes
    $newAuthor.find('[id]').each(function () {
        var $th = $(this);
        if ($th.attr('id') == 'authorsLegend_' + (currentIndex).toString()) {
            $th.html((currentIndex + 2).toString() + "-й автор");
        }
        var newID = $th.attr('id').replace(/\d/, function (str) { return parseInt(str) + 1; });
        $th.attr('id', newID);
    });

    $newAuthor.find('[name]').each(function () {
        var $th = $(this);
        var newName = $th.attr('name').replace(/\d/, function (str) { return parseInt(str) + 1; });
        $th.attr('name', newName);
    });


    $newAuthor.find('[data-valmsg-for]').each(function () {
        var $th = $(this);
        var newdatavalmsg = $th.attr('data-valmsg-for').replace(/\d/, function (str) { return parseInt(str) + 1; });
        $th.attr('data-valmsg-for', newdatavalmsg);
    });

    $newAuthor.find('[tabindex]').each(function () {
        var $th = $(this);
        var tabindexval = $th.attr('tabindex');
        var newtabindex = parseInt(tabindexval) + 12;//;$th.attr('tabindex').replace(/\d/, function (str) { return parseInt(str) + 12; });
        //alert(newtabindex);
        $th.attr('tabindex', newtabindex);
    });

    $newAuthor.find('[for]').each(function () {
        var $th = $(this);
        var newFor = $th.attr('for').replace(/\d/, function (str) { return parseInt(str) + 1; });
        $th.attr('for', newFor);
    });

    //cleaning validation errors
    //that is going to be the new input so there should be no errors
    $newAuthor.find('[class]').each(function () {
        var $th = $(this);
        var classValue = $th.attr('class');
        //console.log(classValue);
        if (classValue == 'field-validation-error')
            $th.attr('class', 'field-validation-valid');
        if (classValue == 'input-validation-error')
            $th.removeAttr('class');
        if (classValue == 'notifyjs-wrapper notifyjs-hidable')
            $th.remove();
    });

    //cleaning all previous values
    $newAuthor.find('input[type="author"]').each(function () {
        var $th = $(this);
        $th.val('');
    });

    var $newSkipBlock = $('#skipBlock_' + currentIndex.toString()).clone();
    $newSkipBlock.attr('id', 'skipBlock_' + (currentIndex + 1).toString());
    var $newAddBtn = $('#addBtn_' + currentIndex.toString()).clone();
    $newAddBtn.attr('id', 'addBtn_' + (currentIndex + 1).toString());
    var $newDelBtn = $('#addBtn_' + currentIndex.toString()).clone();
    $newDelBtn.attr('id', 'removeBtn_' + (currentIndex + 1).toString());
    $newDelBtn.attr('class', 'authorRemoveBtn');
    $newDelBtn.attr("onclick", "confirm('Удалить автора?') && RemoveAuthorFromFieldSet(" + (currentIndex + 1).toString() + ")");
    $newDelBtn.html("Удалить?");
    if (currentIndex == 0) {
        var $newPrevDelBtn = $newDelBtn.clone();
        $newPrevDelBtn.attr("onclick", "confirm('Удалить автора?') && RemoveAuthorFromFieldSet(" + (currentIndex).toString() + ")");
        $('#authorsLegend_' + currentIndex.toString() + ' br').remove();
        $('#authorsLegend_' + currentIndex.toString()).append("<br />").append($newPrevDelBtn);
    }
    //$('#authorsBlock').append($newAuthor).append($newDelBtn).append($newAddBtn).append($newSkipBlock);
    $('#authorsBlock').append($newAuthor).append($newAddBtn).append($newSkipBlock);
    $('#authorsLegend_' + (currentIndex + 1).toString() + ' br').remove();
    $('#authorsLegend_' + (currentIndex + 1).toString()).append("<br />").append($newDelBtn);
    $('#addBtn_' + currentIndex.toString()).remove();

    currentIndex++;
    document.getElementById('authorsIndex').value = currentIndex.toString();
};