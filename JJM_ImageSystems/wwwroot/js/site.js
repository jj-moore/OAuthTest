document.addEventListener('DOMContentLoaded', () => {

  document.getElementById('requestToken').addEventListener('click', () => {
    fetch('Registration/GetToken')
      .then(response => {
        return response.json();
      })
      .then(json => {
        const p = document.createElement('h5');
        p.appendChild(document.createTextNode(json));
        const myDiv = document.getElementById('content');
        myDiv.appendChild(p);
      })
      .catch(error => {
        console.error(error);
      })
  });

  document.getElementById('getSchoolList').addEventListener('click', () => {
    fetch('Registration/GetSchoolList')
      .then(response => {
        return response.json();
      })
      .then(json => {
        const ul = document.createElement('ul');
        for (var i = 0; i < json.length; i++) {
          var li = document.createElement('li');
          li.appendChild(document.createTextNode(json[i].name));
          ul.appendChild(li);
        }
        const myDiv = document.getElementById('content');
        myDiv.appendChild(ul);
      })
      .catch(error => {
        console.error(error);
      })
  });

  document.getElementById('getPsUser').addEventListener('click', () => {
    fetch('Registration/GetPsUser')
      .then(response => {
        return response.json();
      })
      .then(json => {
        const ul = document.createElement('ul');
        for (var i = 0; i < json.length; i++) {
          var li = document.createElement('li');
          li.appendChild(document.createTextNode(`${json[i].dcid} ${json[i].firstName}`));
          ul.appendChild(li);
        }
        const myDiv = document.getElementById('content');
        myDiv.appendChild(ul);
      })
      .catch(error => {
        console.error(error);
      })
  });

});