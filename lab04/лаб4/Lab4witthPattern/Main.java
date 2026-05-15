import javax.swing.SwingUtilities;

// Точка входа: запуск приложения и инициализация GUI в правильном потоке
public class Main {
    public static void main(String[] args) {
        // Явная загрука JDBC-драйвера SQLite (для надёжности)
        try { Class.forName("org.sqlite.JDBC"); } catch (ClassNotFoundException ignored) {}
        
        // Запуск GUI в потоке обработки событий
        SwingUtilities.invokeLater(() -> new GatewayApp().setVisible(true));
    }
}