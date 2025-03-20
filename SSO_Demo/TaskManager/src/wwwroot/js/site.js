// Hàm để tạo công việc mới
async function createTask(e) {
    e.preventDefault();
    
    const title = document.getElementById('taskTitle').value;
    const description = document.getElementById('taskDescription').value;
    const status = parseInt(document.getElementById('taskStatus').value);

    if (!title) {
        alert('Vui lòng nhập tiêu đề công việc');
        return;
    }

    try {
        const response = await fetch('/api/tasks', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                title: title,
                description: description || '',
                status: status
            })
        });

        const data = await response.json();

        if (response.ok) {
            // Đóng modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addTaskModal'));
            modal.hide();
            // Reset form
            document.getElementById('taskForm').reset();
            // Reload trang
            window.location.reload();
        } else {
            let errorMessage = 'Có lỗi xảy ra khi tạo công việc';
            if (data.errors) {
                errorMessage += ': ' + Object.values(data.errors).flat().join(', ');
            } else if (data.message) {
                errorMessage += ': ' + data.message;
            }
            alert(errorMessage);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi tạo công việc: ' + error.message);
    }
}

// Hàm để cập nhật trạng thái công việc
async function moveTask(id, newStatus) {
    try {
        const response = await fetch('/api/tasks/move', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                taskId: id,
                newStatus: newStatus
            })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi cập nhật trạng thái công việc');
        }

        window.location.reload();
    } catch (error) {
        console.error('Error:', error);
        alert(error.message);
    }
}

// Hàm để xóa công việc
async function deleteTask(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa công việc này?')) {
        return;
    }

    try {
        const response = await fetch(`/api/tasks/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi xóa công việc');
        }

        window.location.reload();
    } catch (error) {
        console.error('Error:', error);
        alert(error.message);
    }
}

// Hàm để thêm task vào UI
function addTaskToUI(task) {
    const taskHtml = `
        <div id="task-${task.id}" class="task-item">
            <h5>${task.title}</h5>
            <p>${task.description || ''}</p>
            <div class="task-actions">
                <button class="btn btn-sm btn-primary" onclick="updateTask(${task.id}, ${task.status})">Cập nhật</button>
                <button class="btn btn-sm btn-danger" onclick="deleteTask(${task.id})">Xóa</button>
            </div>
        </div>
    `;

    const statusContainer = document.querySelector(`[data-status="${task.status}"]`);
    if (statusContainer) {
        statusContainer.insertAdjacentHTML('afterbegin', taskHtml);
    }
}

// Hàm để cập nhật task trong UI
function updateTaskInUI(task) {
    const oldTaskElement = document.getElementById(`task-${task.id}`);
    if (oldTaskElement) {
        oldTaskElement.remove();
    }
    addTaskToUI(task);
}

// Hàm để mở modal chỉnh sửa công việc
function editTask(id, title, description) {
    document.getElementById('editTaskId').value = id;
    document.getElementById('editTaskTitle').value = title;
    document.getElementById('editTaskDescription').value = description;
    
    const modal = new bootstrap.Modal(document.getElementById('editTaskModal'));
    modal.show();
}

// Hàm để cập nhật thông tin công việc
async function updateTask(e) {
    e.preventDefault();
    
    const id = document.getElementById('editTaskId').value;
    const title = document.getElementById('editTaskTitle').value;
    const description = document.getElementById('editTaskDescription').value;

    if (!title) {
        alert('Vui lòng nhập tiêu đề công việc');
        return;
    }

    try {
        const response = await fetch(`/api/tasks/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                title: title,
                description: description || ''
            })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi cập nhật công việc');
        }

        // Đóng modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('editTaskModal'));
        modal.hide();
        
        // Hiển thị thông báo thành công
        Toastify({
            text: "Cập nhật công việc thành công",
            duration: 3000,
            close: true,
            gravity: "top",
            position: "right",
            style: {
                background: "#198754"
            }
        }).showToast();

        // Reload trang
        window.location.reload();
    } catch (error) {
        console.error('Error:', error);
        Toastify({
            text: error.message,
            duration: 3000,
            close: true,
            gravity: "top",
            position: "right",
            style: {
                background: "#dc3545"
            }
        }).showToast();
    }
}

// Khởi tạo các event listener khi trang được load
document.addEventListener('DOMContentLoaded', function() {
    // Thêm sự kiện submit cho form tạo mới
    const taskForm = document.getElementById('taskForm');
    if (taskForm) {
        taskForm.addEventListener('submit', createTask);
    }

    // Thêm sự kiện submit cho form chỉnh sửa
    const editTaskForm = document.getElementById('editTaskForm');
    if (editTaskForm) {
        editTaskForm.addEventListener('submit', updateTask);
    }

    // Thêm sự kiện submit cho form cập nhật thông tin cá nhân
    const editProfileForm = document.getElementById('editProfileForm');
    if (editProfileForm) {
        editProfileForm.addEventListener('submit', async function(e) {
            e.preventDefault();

            const formData = new FormData();
            const name = document.getElementById('name').value.trim();
            const email = document.getElementById('email').value.trim();
            
            if (name) formData.append('name', name);
            if (email) formData.append('email', email);

            const avatarInput = document.getElementById('avatar');
            if (avatarInput.files.length > 0) {
                formData.append('avatar', avatarInput.files[0]);
            }

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                const response = await fetch('/api/profile', {
                    method: 'PUT',
                    headers: {
                        'X-CSRF-TOKEN': token,
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': token
                    },
                    body: formData,
                    credentials: 'same-origin'
                });

                const data = await response.json();
                
                if (!response.ok) {
                    throw new Error(data.message || 'Có lỗi xảy ra khi cập nhật thông tin');
                }

                // Hiển thị thông báo thành công
                Toastify({
                    text: data.message,
                    duration: 3000,
                    close: true,
                    gravity: "top",
                    position: "right",
                    style: {
                        background: "#198754"
                    }
                }).showToast();

                // Cập nhật UI
                if (data.success && data.data) {
                    // Cập nhật tên và email trong phần profile
                    document.querySelector('.profile-info h5.my-3').textContent = data.data.name;
                    document.querySelector('.profile-info p.text-muted.mb-1').textContent = data.data.email;
                    
                    // Cập nhật ảnh đại diện nếu có
                    if (data.data.picture) {
                        const avatarImages = document.querySelectorAll('img.rounded-circle');
                        avatarImages.forEach(img => {
                            img.src = data.data.picture;
                        });
                    }

                    // Cập nhật tên trong thanh navigation
                    const navUsername = document.querySelector('.nav-username');
                    if (navUsername) {
                        navUsername.textContent = data.data.name;
                    }

                    // Cập nhật tên ở trang chủ nếu đang ở trang chủ
                    const welcomeMessage = document.querySelector('.display-4.mb-4');
                    if (welcomeMessage && welcomeMessage.textContent.startsWith('Xin chào')) {
                        welcomeMessage.textContent = `Xin chào, ${data.data.name}!`;
                    }

                    // Cập nhật các trường input trong modal
                    document.getElementById('name').value = data.data.name;
                    document.getElementById('email').value = data.data.email;
                }

                // Đóng modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('editProfileModal'));
                modal.hide();
            } catch (error) {
                console.error('Error:', error);
                Toastify({
                    text: error.message,
                    duration: 3000,
                    close: true,
                    gravity: "top",
                    position: "right",
                    style: {
                        background: "#dc3545"
                    }
                }).showToast();
            }
        });
    }
});

// Các hàm tiện ích chung
function formatDate(date) {
    return new Date(date).toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Khởi tạo tooltips và popovers
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});

// Hàm cập nhật ảnh đại diện
function updateProfile() {
    const fileInput = document.getElementById('avatar');
    if (fileInput.files.length > 0) {
        const formData = new FormData();
        formData.append('avatar', fileInput.files[0]);

        fetch('/api/profile/avatar', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Lỗi cập nhật ảnh đại diện');
            }
            return response.json();
        })
        .then(data => {
            Toastify({
                text: "Cập nhật ảnh đại diện thành công",
                duration: 3000,
                close: true,
                gravity: "top",
                position: "right",
                style: {
                    background: "#198754"
                }
            }).showToast();

            // Cập nhật ảnh đại diện trên giao diện
            document.querySelector('img[alt="avatar"]').src = data.url;

            // Đóng modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editProfileModal'));
            modal.hide();
        })
        .catch(error => {
            Toastify({
                text: error.message,
                duration: 3000,
                close: true,
                gravity: "top",
                position: "right",
                style: {
                    background: "#dc3545"
                }
            }).showToast();
        });
    }
} 