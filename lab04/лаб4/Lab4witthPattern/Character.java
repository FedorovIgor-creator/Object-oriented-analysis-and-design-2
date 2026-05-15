public class Character {
    //Модель данных: чистые данные персонажа; формат данных, который шлюз возвращает клиенту
    private final String name, fraction, position, altform, weapon; //final - неизменяемые поля после создания

    public Character(String name, String fraction, String position, String altform, String weapon) {
        this.name = name;
        this.fraction = fraction;
        this.position = position;
        this.altform = altform;
        this.weapon = weapon;
    }

    @Override
    public String toString() {
        return String.format(" %s\n   Сложность: %s | Тип атаки: %s\n   Роль: %s | Характеристики: %s\n",
                name, fraction, position, altform, weapon);
    }
}