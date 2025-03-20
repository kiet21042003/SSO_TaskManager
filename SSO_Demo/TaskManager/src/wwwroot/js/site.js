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

// Khởi tạo các event listener khi trang được load
document.addEventListener('DOMContentLoaded', function() {
    // Thêm sự kiện submit cho form
    const taskForm = document.getElementById('taskForm');
    if (taskForm) {
        taskForm.addEventListener('submit', createTask);
    }

    // Thêm sự kiện click cho nút Lưu trong modal
    const saveButton = document.querySelector('#addTaskModal .btn-primary');
    if (saveButton) {
        saveButton.addEventListener('click', createTask);
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

// Khởi tạo tooltips và popovers của Bootstrap
document.addEventListener('DOMContentLoaded', function() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function(popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}); 