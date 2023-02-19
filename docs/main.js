window.onload = () => {
    let builds = document.querySelectorAll(".build");

    builds.forEach(element => {
        let date = element.getAttribute('date') || "Unreleased";
        let version = element.getAttribute('version') || "X.X.X";
        let tags = element.getAttribute('tags') || "";

        let tagsList = tags.trim().split(' ');
        let tagsHTML = '';
        tagsList.forEach(tag => {
            tagsHTML += `<p class="${tag} tag">${tag}</p>`
        });

        element.innerHTML = `
            <p><a href="./v${version}/"><strong>v${version}</strong></a> [${date}]</p>
            <div class="tag-list">${tagsHTML}</div>
        ` + element.innerHTML;
    });
}