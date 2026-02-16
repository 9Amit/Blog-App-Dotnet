const API_URL = "http://localhost:5205/api/posts";

async function fetchPosts() {
  const response = await fetch(API_URL);
  const posts = await response.json();

  const container = document.getElementById("posts");
  container.innerHTML = "";

  posts.forEach((post) => {
    const div = document.createElement("div");
    div.className = "post";

    div.innerHTML = `
            <h3>${post.title}</h3>
            <p>${post.content}</p>
            <small>${new Date(post.createdAt).toLocaleString()}</small>
            <br><br>
            <button onclick="deletePost(${post.id})">Delete</button>
        `;

    container.appendChild(div);
  });
}

async function createPost() {
  const title = document.getElementById("title").value;
  const content = document.getElementById("content").value;

  if (!title || !content) {
    alert("Title and content required");
    return;
  }

  await fetch(API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      title: title,
      content: content,
    }),
  });

  document.getElementById("title").value = "";
  document.getElementById("content").value = "";

  fetchPosts();
}

async function deletePost(id) {
  await fetch(`${API_URL}/${id}`, {
    method: "DELETE",
  });

  fetchPosts();
}

fetchPosts();
