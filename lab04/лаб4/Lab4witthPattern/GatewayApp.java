import javax.swing.*;
import java.awt.*;
import java.util.List;

/**
 * Клиент. Знает ТОЛЬКО о шлюзе и его публичных методах. Не содержит ни одной строки SQL, JDBC или ResultSet.
 */
public class GatewayApp extends JFrame {
    private final CharacterGateway gateway; //зависимость только от шлюза
    private JTextField inputField;
    private JTextArea outputArea;

    public GatewayApp() {
        this.gateway = new CharacterGateway();
        setupUI();
    }

    private void setupUI() {
        // функция настройки окна, кнопок и полей
        setTitle("Dota 2 Heroes Q&A (С паттерном Gateway)");
        setSize(580, 460);
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        setLocationRelativeTo(null);
        setLayout(new BorderLayout(10, 10));

        // Панель управления
        JPanel topPanel = new JPanel(new FlowLayout());
        inputField = new JTextField(22);
        JButton btnSearch = new JButton("Найти по имени");
        JButton btnListAll = new JButton("Список всех");
        topPanel.add(new JLabel("Имя персонажа:"));
        topPanel.add(inputField);
        topPanel.add(btnSearch);
        topPanel.add(btnListAll);
        add(topPanel, BorderLayout.NORTH);

        // Область вывода
        outputArea = new JTextArea();
        outputArea.setEditable(false);
        outputArea.setFont(new Font("Consolas", Font.PLAIN, 14));
        outputArea.setMargin(new Insets(10, 10, 10, 10));
        add(new JScrollPane(outputArea), BorderLayout.CENTER);

        // Логика кнопок: ДЕЛЕГИРОВАНИЕ ШЛЮЗУ
        btnSearch.addActionListener(e -> {
            String name = inputField.getText().trim();
            if (name.isEmpty()) return;
            outputArea.setText("Поиск через шлюз...\n");
            Character result = gateway.findByName(name); //делегирование шлюзу для подключения к бд
            outputArea.setText(result != null ? " *" + result : "Персонаж не найден в архиве.");
        });

        btnListAll.addActionListener(e -> {
            outputArea.setText(" Загрузка списка через шлюз...\n");
            List<Character> list = gateway.findAll(); //делегирование
            StringBuilder sb = new StringBuilder("Все персонажи:\n" + "─".repeat(50) + "\n");
            list.forEach(c -> sb.append(c).append("\n"));
            outputArea.setText(sb.toString());
        });
    }
}