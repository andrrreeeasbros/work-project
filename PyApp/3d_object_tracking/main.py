import tkinter as tk
from tkinter import messagebox, filedialog
import cv2
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from PIL import Image, ImageTk

# Глобальные переменные для хранения координат и цветов
x_coords = np.random.random(10)
y_coords = np.random.random(10)
z_coords = np.random.random(10)
colors = ['b'] * 10  # Все точки синие по умолчанию

# Глобальные переменные для отслеживания минимальных и максимальных значений координат
x_min, x_max = 0, 1
y_min, y_max = 0, 1
z_min, z_max = 0, 1

# Инициализация детектора для человека (HOG)
hog = cv2.HOGDescriptor()
hog.setSVMDetector(cv2.HOGDescriptor_getDefaultPeopleDetector())

# Флаг для остановки видео
video_playing = False
cap = None  # Для захвата видео
video_length = 0  # Длина видео в кадрах

# Переменные для отслеживания предыдущих координат объекта
prev_head_x = None
prev_head_y = None
motion_threshold = 20  # Порог для определения движения (в пикселях)

def capture_video(video_path):
    global x_coords, y_coords, z_coords, x_min, x_max, y_min, y_max, z_min, z_max, cap, video_playing, colors, video_length  # Обновляем глобальные переменные
    global prev_head_x, prev_head_y, prev_head_frame  # Добавляем переменную для отслеживания предыдущего кадра
    global motion_threshold, motion_counter, max_static_frames  # Обновляем переменные для контроля за движением

    cap = cv2.VideoCapture(video_path)  # Открытие видеофайла

    if not cap.isOpened():
        print("Ошибка: Видео не найдено!")
        messagebox.showerror("Ошибка", "Не удается подключиться к видео.")
        return
    else:
        print(f"Видео '{video_path}' успешно подключено")

    # Получаем размер холста
    canvas_width = canvas.winfo_width()
    canvas_height = canvas.winfo_height()

    # Получаем длину видео в кадрах
    video_length = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    # Устанавливаем начальные значения для перемотки
    scale_video.config(to=video_length - 1, sliderlength=20, length=600)

    video_playing = True  # Устанавливаем флаг видео в состояние "играет"
    
    motion_counter = 0  # Счётчик кадров, в которых не было движения
    max_static_frames = 10  # Максимум кадров без движения перед игнорированием объекта

    while video_playing:
        ret, frame = cap.read()
        if not ret:
            print("Не удается захватить кадр.")
            break

        # Детекция людей на изображении
        boxes, weights = hog.detectMultiScale(frame, winStride=(8, 8), padding=(8, 8), scale=1.05)

        for (x, y, w, h) in boxes:
            # Нарисовать прямоугольник вокруг обнаруженного человека
            cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)  # Зеленый прямоугольник

            # Центр головы (верхняя часть прямоугольника)
            head_x = x + w // 2
            head_y = y

            # Проверяем, есть ли движение
            if prev_head_x is not None and prev_head_y is not None:
                # Вычисляем расстояние между предыдущими и текущими координатами
                dist = np.sqrt((head_x - prev_head_x) ** 2 + (head_y - prev_head_y) ** 2)
                
                # Если движение превышает порог, обновляем положение объекта
                if dist >= motion_threshold:
                    color = 'r'  # Если есть движение, устанавливаем красный цвет
                    # Сбрасываем счётчик статичных кадров
                    motion_counter = 0
                    # Обновляем координаты
                    x_coords = np.append(x_coords[1:], head_x / canvas_width)
                    y_coords = np.append(y_coords[1:], head_y / canvas_height)
                    z_coords = np.append(z_coords[1:], np.random.random())
                else:
                    color = 'b'  # Если движения нет, ставим синий цвет
                    motion_counter += 1
                    # Проверка на слишком много кадров без движения
                    if motion_counter > max_static_frames:
                        # Если слишком долго не было движения, игнорируем координаты
                        continue
            else:
                # Первоначальная точка (нет предыдущих координат)
                color = 'b'
                # Обновляем координаты только один раз при первой детекции
                x_coords = np.append(x_coords[1:], head_x / canvas_width)
                y_coords = np.append(y_coords[1:], head_y / canvas_height)
                z_coords = np.append(z_coords[1:], np.random.random())

            # Обновляем цвета точек
            colors = [color] * len(x_coords)  # Меняем цвет точек в зависимости от движения

            # Обновляем минимальные и максимальные значения координат
            x_min = min(x_min, np.min(x_coords))
            x_max = max(x_max, np.max(x_coords))
            y_min = min(y_min, np.min(y_coords))
            y_max = max(y_max, np.max(y_coords))
            z_min = min(z_min, np.min(z_coords))
            z_max = max(z_max, np.max(z_coords))

            # Обновляем метки с новыми координатами
            label_coordinates.config(text=f"X: {x_coords[-1]:.2f} | Y: {y_coords[-1]:.2f} | Z: {z_coords[-1]:.2f}")

            # Обновляем 3D график
            plot_3d_coordinates()

            # Обновляем предыдущие координаты
            prev_head_x, prev_head_y = head_x, head_y

            # Отображаем координаты объекта на видео, если это движущийся объект (красный)
            if color == 'r':
                # Текст с координатами
                coord_text = f"({head_x}, {head_y})"
                cv2.putText(frame, coord_text, (x, y - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2, cv2.LINE_AA)

        # Масштабируем кадр, чтобы он помещался в холст
        frame_resized = cv2.resize(frame, (canvas.winfo_width(), canvas.winfo_height()))

        # Преобразуем изображение в формат для Tkinter
        frame_rgb = cv2.cvtColor(frame_resized, cv2.COLOR_BGR2RGB)
        frame_img = Image.fromarray(frame_rgb)
        frame_tk = ImageTk.PhotoImage(image=frame_img)

        # Обновляем изображение в Canvas
        canvas.create_image(0, 0, anchor=tk.NW, image=frame_tk)
        canvas.image = frame_tk  # Сохраняем ссылку на изображение, чтобы оно не исчезло

        window.update_idletasks()
        window.update()

    cap.release()



def plot_3d_coordinates():
    global x_coords, y_coords, z_coords, scatter_plot, ax, fig, x_min, x_max, y_min, y_max, z_min, z_max, colors

    # Если график еще не был создан, создаем его
    if scatter_plot is None:
        fig = plt.figure(figsize=(5, 4))
        ax = fig.add_subplot(111, projection='3d')
        scatter_plot = ax.scatter(x_coords, y_coords, z_coords, c=colors)
        ax.set_xlabel('X')
        ax.set_ylabel('Y')
        ax.set_zlabel('Z')

        # Устанавливаем динамические границы осей, чтобы они адаптировались под координаты
        ax.set_xlim([x_min, x_max])
        ax.set_ylim([y_min, y_max])
        ax.set_zlim([z_min, z_max])

        # Создаем 3D холст в Tkinter
        canvas_3d = FigureCanvasTkAgg(fig, master=frame_3d)
        canvas_3d.draw()
        canvas_3d.get_tk_widget().pack()

        # Показываем контейнер с координатами (когда 3D график появляется)
        frame_tracking_info.pack(side=tk.TOP, fill=tk.X, pady=5)
    else:
        # Обновляем 3D график, если он уже был создан
        scatter_plot._offsets3d = (x_coords, y_coords, z_coords)
        scatter_plot.set_color(colors)  # Обновляем цвета точек
        ax.set_xlim([x_min, x_max])
        ax.set_ylim([y_min, y_max])
        ax.set_zlim([z_min, z_max])
        fig.canvas.draw_idle()

    # Обновляем координаты, отображаем контейнер с координатами
    if x_coords.size > 0:  # Если есть хотя бы одна точка, показываем координаты
        label_coordinates.config(text=f"X: {x_coords[-1]:.2f} | Y: {y_coords[-1]:.2f} | Z: {z_coords[-1]:.2f}")
        frame_tracking_info.pack(side=tk.TOP, fill=tk.X, pady=5)
    else:
        # Скрываем контейнер с координатами, если нет данных
        frame_tracking_info.pack_forget()

video_playing = False  # глобальная переменная для отслеживания состояния воспроизведения

# Функция для начала/остановки видео
def toggle_video():
    global video_playing
    video_path = video_path_entry.get()  # Получаем путь к видео из текстового поля

    # Проверка, добавлен ли путь к видео
    if not video_path:
        messagebox.showerror("Ошибка", "Вы не добавили видео. Пожалуйста, выберите файл видео.")
        return

    if not video_playing:
        start_button.config(text='Stop', bg='#d9534f', fg='white', relief=tk.RAISED, bd=2)  # Красная кнопка
        capture_video(video_path)  # Запуск захвата видео
    elif video_playing:
        stop_video()

# Функция для остановки видео
def stop_video():
    global video_playing
    video_playing = False
    start_button.config(text='Continue', bg='#5bc0de', fg='white', relief=tk.RAISED, bd=2)  # Кнопка "Continue" голубая

# Функция для продолжения видео
def continue_video():
    global video_playing
    video_playing = True
    start_button.config(text='Stop', bg='#d9534f', fg='white', relief=tk.RAISED, bd=2)  # Кнопка "Stop" красная
    video_path = video_path_entry.get()
    capture_video(video_path)  # Продолжаем воспроизведение

# Функция для перемотки видео при изменении ползунка
def update_video_position(val):
    if cap and cap.isOpened():
        # Перемещаем видео на указанный кадр
        cap.set(cv2.CAP_PROP_POS_FRAMES, int(val))
        if video_playing:
            ret, frame = cap.read()
            if ret:
                # Масштабируем кадр, чтобы он помещался в холст
                frame_resized = cv2.resize(frame, (canvas.winfo_width(), canvas.winfo_height()))
                # Преобразуем изображение в формат для Tkinter
                frame_rgb = cv2.cvtColor(frame_resized, cv2.COLOR_BGR2RGB)
                frame_img = Image.fromarray(frame_rgb)
                frame_tk = ImageTk.PhotoImage(image=frame_img)
                # Обновляем изображение в Canvas
                canvas.create_image(0, 0, anchor=tk.NW, image=frame_tk)
                canvas.image = frame_tk  # Сохраняем ссылку на изображение, чтобы оно не исчезло

            # Обновляем метку времени
            frame_pos = int(val)  # Текущая позиция в кадрах
            time_display = convert_frames_to_time(frame_pos)
            label_time.config(text=f"Time: {time_display}")

def convert_frames_to_time(frame_num):
    # Получаем FPS (frames per second) видео
    fps = cap.get(cv2.CAP_PROP_FPS)
    
    # Рассчитываем время на основе номера кадра
    seconds = int(frame_num / fps)
    hours = seconds // 3600
    minutes = (seconds % 3600) // 60
    seconds = seconds % 60

    # Возвращаем время в формате "HH:MM:SS"
    return f"{hours:02}:{minutes:02}:{seconds:02}"


# Функция для выбора видеофайла через диалоговое окно
def browse_video():
    file_path = filedialog.askopenfilename(filetypes=[("Video Files", "*.mp4 *.avi *.mov *.mkv")])
    if file_path:
        video_path_entry.delete(0, tk.END)  # Очистить текущее значение
        video_path_entry.insert(0, file_path)  # Вставить выбранный путь
        browse_button.config(bg='#f0ad4e', fg='white', relief=tk.RAISED, bd=2)  # Изменяем стиль кнопки


def change_video():
    # Проверка, если путь к видео не указан
    if not video_path_entry.get():  # Если поле пути пустое
        messagebox.showerror("Ошибка", "Путь не добавлен. Нечего менять!")
        return  # Прерываем выполнение функции
    
    file_path = filedialog.askopenfilename(filetypes=[("Video Files", "*.mp4 *.avi *.mov *.mkv")])
    if file_path:
        video_path_entry.delete(0, tk.END)  # Очистить текущее значение
        video_path_entry.insert(0, file_path)  # Вставить выбранный путь
        browse_button.config(bg='#f0ad4e', fg='white', relief=tk.RAISED, bd=2)  # Изменяем стиль кнопки Browse
        start_button.config(text="Start", bg='#28a745', fg='white', relief=tk.RAISED, bd=2)  # Сменить текст на Start


# Функция для выхода из программы
def exit_program():
    if messagebox.askokcancel("Выход", "Вы уверены, что хотите выйти?"):
        window.quit()

def style_controls():
    # Стилизация кнопок
    start_button.config(
        font=("Arial", 12, 'bold'),
        bg='#28a745',  # Зелёный цвет для кнопки Start
        fg='white', 
        relief=tk.RAISED, 
        bd=2,
        width=12
    )
    browse_button.config(
        font=("Arial", 12, 'bold'),
        bg='#ffc107',  # Желтый для кнопки Browse
        fg='black',
        relief=tk.RAISED,
        bd=2,
        width=12
    )
    
    # Стилизация кнопки Exit
    exit_button.config(
        font=("Arial", 12, 'bold'),
        bg='#dc3545',  # Красный цвет для кнопки Exit
        fg='white',
        relief=tk.RAISED,
        bd=2,
        width=12
    )
    
    # Стилизация полей ввода
    video_path_entry.config(
        font=("Arial", 12, 'bold'),  # Жирный шрифт для текста
        relief=tk.SUNKEN,
        bd=2,
        width=50,
        fg="black",  # Черный цвет текста
        bg="#f4f4f4"  # Светлый фон для поля ввода
    )
    video_path_label.config(
        font=("Arial", 12, 'bold'),
        bg='#007bff',  # Синий фон для метки
        fg='white'  # Белый цвет текста для контраста
    )

def style_video_path_container():
    # Стилизация контейнера для пути (синий фон)
    frame_video_path.config(
        bg='#007bff',  # Синий цвет для контейнера
        bd=2,
        relief=tk.RAISED
    )

def style_slider():
    scale_video.config(
        bg='#f8f9fa', 
        sliderlength=20, 
        width=20, 
        relief=tk.FLAT, 
        bd=0,
        troughcolor="#e0e0e0"  # Светлый цвет канала ползунка
    )

def style_coordinates():
    label_coordinates.config(
        font=("Arial", 14, 'bold'),
        bg='#f8f9fa',
        fg='#495057',  # Темно-серый для текста координат
        anchor='w',
        padx=10
    )

def style_3d_plot():
    frame_3d.config(bg='#f0f8ff', padx=20, pady=20)  # Светло-голубой фон для панели 3D
    status_label.config(
        font=("Arial", 12, 'bold'),
        fg="#28a745",  # Зеленый цвет для активного статуса
        bg="#f0f8ff",
        anchor="w"
    )

# Инициализация интерфейса
window = tk.Tk()
window.title("3D Object Tracking System")

# Основной фон окна
window.config(bg='#343a40')  # Темно-серый фон для всего окна

frame_title = tk.Frame(window, bg='#343a40')  # Панель с темным фоном для заголовка
frame_title.pack(side=tk.TOP, fill=tk.X, pady=10)

# Заголовок 
title_label = tk.Label(window, text="Приложение N2", font=("Arial", 24, 'bold'), fg='#ff4500', bg='#343a40')
title_label.pack(side=tk.TOP, pady=(0, 10))  # Паддинг, чтобы отодвинуть немного вниз

# Панель управления (для других элементов)
frame_controls = tk.Frame(window, bg='#343a40')  # Панель управления с темным фоном
frame_controls.pack(side=tk.TOP, fill=tk.X, padx=0, pady=10)

# Кнопки Start и Browse в одном контейнере для размещения их сверху слева
frame_buttons = tk.Frame(window, bg='#343a40')  # Новый контейнер для кнопок с темным фоном
frame_buttons.pack(side=tk.TOP, anchor="w", padx=20, pady=10)

# Кнопка для старта/остановки видео
start_button = tk.Button(frame_buttons, text="Start", command=toggle_video, bg='#28a745', fg='white', relief=tk.RAISED, bd=4, width=12)
start_button.pack(side=tk.LEFT, padx=10)

# Кнопка для выбора видеофайла
browse_button = tk.Button(frame_buttons, text="Browse", command=browse_video, bg='#ffc107', fg='black', relief=tk.RAISED, bd=4, width=12)
browse_button.pack(side=tk.LEFT, padx=10)

# Кнопка для изменения видео
change_button = tk.Button(frame_buttons, text="Change Video", command=change_video, bg='#17a2b8', fg='white', relief=tk.RAISED, bd=4, width=15)
change_button.pack(side=tk.LEFT, padx=10)


# Контейнер для поля ввода пути с голубым фоном
frame_video_path = tk.Frame(window, bg='#f0f8ff', bd=2, relief=tk.RAISED)  # Новый контейнер для пути
frame_video_path.pack(side=tk.TOP, anchor="w", padx=20, pady=10)  # Увеличиваем отступ слева (padx=20)

# Метка для пути
video_path_label = tk.Label(frame_video_path, text="Video Path:", font=("Arial", 12), bg='#007bff', fg='white')  # Синий фон для метки
video_path_label.pack(side=tk.LEFT, padx=10)

# Поле для ввода пути к видеофайлу
video_path_entry = tk.Entry(frame_video_path, width=50, font=("Arial", 12, 'bold'))  # Жирный текст
video_path_entry.pack(side=tk.LEFT, padx=10)

# Кнопка Exit, размещённая в правом верхнем углу
exit_button = tk.Button(window, text="Exit", command=exit_program)
exit_button.place(relx=1.0, rely=0.0, anchor='ne', x=-10, y=60) 

# Окно видео
frame_video = tk.Frame(window)
frame_video.pack(side=tk.LEFT, padx=20, pady=10)  # Убираем лишние отступы

# Подпись над видео
video_label = tk.Label(frame_video, text="Video", font=("Arial", 12, 'bold'), bg='#f0f8ff', fg='black')
video_label.pack(side=tk.TOP, fill=tk.X, pady=5)

# Панель с ползунком для перемотки видео (над видео)
frame_slider = tk.Frame(frame_video)
frame_slider.pack(side=tk.TOP, fill=tk.X, pady=5)  # Убираем лишние отступы

# Ползунок для перемотки видео
scale_video = tk.Scale(frame_slider, from_=0, to=100, orient=tk.HORIZONTAL, length=600, command=update_video_position)
scale_video.pack(side=tk.LEFT, fill=tk.X)

canvas = tk.Canvas(frame_video, width=700, height=500)
canvas.pack(fill=tk.BOTH, expand=True)  # Подстраиваем холст под размер окна

# Панель 3D визуализации
frame_3d = tk.Frame(window)
frame_3d.pack(side=tk.RIGHT, padx=20, pady=10)

# Подпись над визуализатором
visualizer_label = tk.Label(frame_3d, text="SpaceVisualizer", font=("Arial", 12, 'bold'), bg='#f0f8ff', fg='black')
visualizer_label.pack(side=tk.TOP, fill=tk.X, pady=5)

# Переносим блок с метками координат и статусом в frame_3d
frame_tracking_info = tk.Frame(frame_3d, bg='#f0f8ff')  # Теперь контейнер для информации о статусе и координатах будет внутри frame_3d
frame_tracking_info.pack(side=tk.TOP, fill=tk.X, pady=5)  # Расположим его прямо над 3D визуализацией

# Статус индикатор
status_label = tk.Label(frame_tracking_info, text="Tracking Active", fg="#28a745", font=("Arial", 12, 'bold'))
status_label.pack(side=tk.LEFT, padx=10)

# Таблица с координатами
label_coordinates = tk.Label(frame_tracking_info, text="X: -- | Y: -- | Z: --", font=("Arial", 14, 'bold'))
label_coordinates.pack(side=tk.LEFT, padx=10)

# Глобальные переменные для графика
scatter_plot = None
ax = None
fig = None

# Применение стилей после создания виджетов
style_controls()
style_slider()
style_coordinates()
style_3d_plot()
style_video_path_container()  # Применение стиля для контейнера пути

# Запуск интерфейса
window.mainloop()


