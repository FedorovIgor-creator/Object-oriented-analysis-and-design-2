import tkinter as tk
from tkinter import messagebox, scrolledtext
from abc import ABC, abstractmethod

#  ПАТТЕРН: ЦЕПОЧКА ОБЯЗАННОСТЕЙ


class AuthHandler(ABC):
    """Базовый класс обработчика аутентификации."""
    def __init__(self):
        self.next_handler = None

    def set_next(self, handler):
        """Связывает текущего обработчика со следующим."""
        self.next_handler = handler
        return handler

    @abstractmethod
    def check(self, request: dict) -> bool:
        """Метод проверки. Возвращает True, если проверка пройдена, и вызывает следующий."""
        pass

    def handle(self, request: dict, log_func) -> bool:
        """Основной метод выполнения цепочки."""
        result = self.check(request)
        if not result:
            return False
        # Если проверка прошла успешно, передаем дальше, если есть следующий
        if self.next_handler:
            return self.next_handler.handle(request, log_func)
        return True

# Конкретные обработчики

class BanChecker(AuthHandler):
    def check(self, request: dict) -> bool:
        username = request.get('username')
        if username.lower() == 'admin_banned':
            request['error'] = "Пользователь заблокирован администрацией!"
            return False
        print("BanChecker: OK")
        request['log'].append("✅ Проверка бана: пройдена")
        return True

class CredentialChecker(AuthHandler):
    def check(self, request: dict) -> bool:
        if request.get('password') != '12345':
            request['error'] = "Неверный пароль!"
            return False
        print("CredentialChecker: OK")
        request['log'].append("✅ Проверка логина/пароля: пройдена")
        return True

class TwoFactorChecker(AuthHandler):
    def check(self, request: dict) -> bool:
        # Этот шаг активен только если включен 2FA
        if not request.get('2fa_enabled'):
            request['log'].append("ℹ️ 2FA отключен, пропуск проверки")
            return True
            
        token = request.get('token')
        if token != '777':
            request['error'] = "Неверный код 2FA!"
            return False
        print("TwoFactorChecker: OK")
        request['log'].append("✅ Проверка 2FA: пройдена")
        return True

# GUI 


class AuthAppGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("Chain of Responsibility: Auth System")
        self.root.geometry("600x500")
        self.root.configure(bg="#f0f0f0")

        self._build_chain()
        self._create_ui()

    def _build_chain(self):
        """Сборка цепочки обработчиков."""
        self.ban_checker = BanChecker()
        self.cred_checker = CredentialChecker()
        self.two_fa_checker = TwoFactorChecker()

        # Выстраиваем цепочку: Ban -> Credentials -> 2FA
        self.ban_checker.set_next(self.cred_checker).set_next(self.two_fa_checker)

    def _create_ui(self):
        #Настройки цепи (слева)
        settings_frame = tk.LabelFrame(self.root, text="Настройки цепи", padx=10, pady=10)
        settings_frame.pack(fill=tk.X, padx=10, pady=5)

        self.var_ban = tk.BooleanVar(value=True)
        self.var_cred = tk.BooleanVar(value=True)
        self.var_2fa = tk.BooleanVar(value=False)

        tk.Checkbutton(settings_frame, text="Активировать проверку Бана", variable=self.var_ban).pack(anchor='w')
        tk.Checkbutton(settings_frame, text="Активировать проверку Пароля", variable=self.var_cred).pack(anchor='w')
        tk.Checkbutton(settings_frame, text="Активировать 2FA (SMS)", variable=self.var_2fa).pack(anchor='w')
        
        tk.Label(settings_frame, text="Правильный пароль: 12345, 2FA код: 777", fg="gray").pack(anchor='w', pady=(5,0))

        #Форма входа (справа)
        form_frame = tk.LabelFrame(self.root, text="Вход в систему", padx=10, pady=10)
        form_frame.pack(fill=tk.X, padx=10, pady=5)

        tk.Label(form_frame, text="Логин:").grid(row=0, column=0, sticky='w')
        self.entry_user = tk.Entry(form_frame)
        self.entry_user.grid(row=0, column=1, padx=5)
        self.entry_user.insert(0, "user_test")

        tk.Label(form_frame, text="Пароль:").grid(row=1, column=0, sticky='w')
        self.entry_pass = tk.Entry(form_frame, show="*")
        self.entry_pass.grid(row=1, column=1, padx=5)
        self.entry_pass.insert(0, "12345")

        tk.Label(form_frame, text="Код 2FA:").grid(row=2, column=0, sticky='w')
        self.entry_token = tk.Entry(form_frame)
        self.entry_token.grid(row=2, column=1, padx=5)
        self.entry_token.insert(0, "777")

        tk.Button(form_frame, text="ВОЙТИ", command=self.perform_login, bg="#4CAF50", fg="white", font=("Arial", 10, "bold")).grid(row=3, column=1, pady=10)

        # Лог (внизу)
        log_frame = tk.LabelFrame(self.root, text="Лог обработки запроса", padx=10, pady=10)
        log_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=5)

        self.log_text = scrolledtext.ScrolledText(log_frame, height=12, bg="#222", fg="#0f0", font=("Consolas", 10))
        self.log_text.pack(fill=tk.BOTH)

    def perform_login(self):
        self.log_text.delete(1.0, tk.END)
        
        # Собираем запрос
        request = {
            'username': self.entry_user.get(),
            'password': self.entry_pass.get(),
            'token': self.entry_token.get(),
            '2fa_enabled': self.var_2fa.get(),
            'log': []
        }

        self._log("🚀 Запрос на авторизацию отправлен в цепочку...")
        self._log(f"Данные: {request['username']}, 2FA: {'ON' if request['2fa_enabled'] else 'OFF'}")

        try:
 
            current_handler = self.ban_checker
            
            # Если галочка "Бан" снята, пропускаем это звено
            if not self.var_ban.get():
                current_handler = current_handler.next_handler # Перепрыгиваем на Credentials
                self._log("⚙️ Проверка бана исключена из цепи динамически")
            
            # Если галочка "Пароль" снята (редко, но возможно для демо)
            if not self.var_cred.get():
                current_handler = current_handler.next_handler
                self._log("⚙️ Проверка пароля исключена из цепи динамически")

            # Запускаем цепочку
            if current_handler:
                success = current_handler.handle(request, self._log)
                
                # Вывод лога из запроса
                for line in request['log']:
                    self._log(line)

                if success:
                    self._log("\n🎉 ДОСТУП РАЗРЕШЕН! Добро пожаловать.")
                else:
                    self._log(f"\n❌ ДОСТУП ЗАПРЕЩЕН: {request['error']}")
            else:
                self._log("⚠️ Цепочка пуста, доступ разрешен по умолчанию.")

        except Exception as e:
            self._log(f"\n💥 Ошибка системы: {str(e)}")

    def _log(self, message):
        self.log_text.insert(tk.END, message + "\n")
        self.log_text.see(tk.END)

if __name__ == "__main__":
    root = tk.Tk()
    app = AuthAppGUI(root)
    root.mainloop()