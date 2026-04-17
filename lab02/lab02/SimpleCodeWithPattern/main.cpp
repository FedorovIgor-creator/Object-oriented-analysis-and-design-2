#include <windows.h>
#include <string>
#include <map>
#include <vector>
#include <sstream>

// ==================== TEXT STYLE ====================

class TextStyle {
private:
    std::string fontName;
    float fontSize;
    bool isBold;

public:
    TextStyle(const std::string& font = "Arial", float size = 12.0f, bool bold = false)
        : fontName(font), fontSize(size), isBold(bold) {
    }

    std::string getInfo() const {
        return fontName + " " + std::to_string((int)fontSize) + (isBold ? " Bold" : "");
    }

    std::string getKey() const {
        return fontName + "|" + std::to_string((int)fontSize) + "|" + (isBold ? "1" : "0");
    }
};

// ==================== CHARACTER (FLYWEIGHT) ====================

class Character {
private:
    static int objectCount;
    std::string symbol;
    TextStyle* style;

public:
    Character(const std::string& s, TextStyle* st) : symbol(s), style(st) {
        objectCount++;
    }

    static int getObjectCount() { return objectCount; }
    static void resetCount() { objectCount = 0; }

    std::string getSymbol() const { return symbol; }
    TextStyle* getStyle() const { return style; }

    void draw(int x, int y) const {
        // Рендеринг символа в позиции (x, y)
    }

    std::string getInfo() const {
        return "'" + symbol + "' [" + style->getInfo() + "]";
    }
};

int Character::objectCount = 0;

// ==================== CHARACTER FACTORY ====================

class CharacterFactory {
private:
    std::map<std::string, Character*> cache;
    int reuseCount;

public:
    CharacterFactory() : reuseCount(0) {}

    ~CharacterFactory() {
        for (auto& pair : cache) delete pair.second;
    }

    Character* getCharacter(const std::string& key, int size, const std::string& fontName) {
        std::string cacheKey = key + "|" + fontName + "|" + std::to_string(size);

        if (cache.find(cacheKey) != cache.end()) {
            reuseCount++;
            return cache[cacheKey];
        }

        TextStyle* style = new TextStyle(fontName, (float)size, false);
        Character* ch = new Character(key, style);
        cache[cacheKey] = ch;
        return ch;
    }

    int getUniqueCount() const { return cache.size(); }
    int getReuseCount() const { return reuseCount; }

    std::string getAllInfo() const {
        std::stringstream ss;
        ss << "=== Characters ===\r\n";
        for (const auto& pair : cache) {
            ss << "- " << pair.second->getInfo() << "\r\n";
        }
        return ss.str();
    }
};

// ==================== TEXT POINT ====================

class TextPoint {
private:
    int cord_X;
    int cord_Y;
    Character* character;

public:
    TextPoint(int x, int y, Character* ch) : cord_X(x), cord_Y(y), character(ch) {}

    int getX() const { return cord_X; }
    int getY() const { return cord_Y; }
    Character* getCharacter() const { return character; }

    void render() const {
        character->draw(cord_X, cord_Y);
    }

    std::string getInfo() const {
        return "[" + std::to_string(cord_X) + "," + std::to_string(cord_Y) + "] " + character->getInfo();
    }
};

// ==================== TEXT RENDER ====================

class TextRender {
private:
    std::vector<TextPoint*> points;

public:
    void addTextPoint(TextPoint* t) {
        points.push_back(t);
    }

    void renderAll() const {
        for (const auto& p : points) {
            p->render();
        }
    }

    int getCount() const { return points.size(); }

    std::string getAllInfo() const {
        std::stringstream ss;
        ss << "=== Text Points ===\r\n";
        for (const auto& p : points) {
            ss << "- " << p->getInfo() << "\r\n";
        }
        return ss.str();
    }

    ~TextRender() {
        for (auto& p : points) delete p;
    }
};

// ==================== GLOBAL VARIABLES ====================

CharacterFactory* factory = nullptr;
TextRender* render = nullptr;

HWND hEditInfo = NULL;
HWND hEditStats = NULL;
HWND hComboSymbol = NULL;
HWND hComboFont = NULL;
HWND hComboSize = NULL;
HWND hEditX = NULL;
HWND hEditY = NULL;

int totalRequests = 0;

// ==================== GUI FUNCTIONS ====================

void UpdateAll() {
    if (!hEditInfo || !hEditStats || !factory || !render) return;

    std::string info = factory->getAllInfo() + "\r\n" + render->getAllInfo();
    SetWindowTextA(hEditInfo, info.c_str());

    int unique = factory->getUniqueCount();
    int total = render->getCount();
    int saved = total - unique;

    std::stringstream ss;
    ss << "=== Statistics ===\r\n\r\n";
    ss << "Unique Characters: " << unique << "\r\n";
    ss << "Total TextPoints: " << total << "\r\n";
    ss << "Factory Reuse: " << factory->getReuseCount() << "\r\n\r\n";
    ss << "Memory saved: " << (saved > 0 ? saved : 0) << " object(s)";

    SetWindowTextA(hEditStats, ss.str().c_str());
}

void OnAdd() {
    if (!factory || !render) return;

    char symbol[2] = { 0 };
    char font[32] = { 0 };
    char size[16] = { 0 };
    char xStr[16] = { 0 };
    char yStr[16] = { 0 };

    GetWindowTextA(hComboSymbol, symbol, 2);
    GetWindowTextA(hComboFont, font, 32);
    GetWindowTextA(hComboSize, size, 16);
    GetWindowTextA(hEditX, xStr, 16);
    GetWindowTextA(hEditY, yStr, 16);

    if (symbol[0] == 0 || size[0] == 0 || xStr[0] == 0 || yStr[0] == 0) {
        MessageBoxA(NULL, "Fill all fields!", "Error", MB_ICONWARNING);
        return;
    }

    int x = atoi(xStr);
    int y = atoi(yStr);
    int sz = atoi(size);

    Character* ch = factory->getCharacter(std::string(1, symbol[0]), sz, font);
    TextPoint* point = new TextPoint(x, y, ch);

    render->addTextPoint(point);
    totalRequests++;

    UpdateAll();
}

void OnClear() {
    delete factory;
    delete render;
    factory = new CharacterFactory();
    render = new TextRender();
    Character::resetCount();
    totalRequests = 0;

    SetWindowTextA(hEditInfo, "");
    UpdateAll();
}

// ==================== WINDOW PROCEDURE ====================

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp) {
    switch (msg) {
    case WM_CREATE: {
        CreateWindowA("STATIC", "Flyweight Pattern - Diagram Match",
            WS_VISIBLE | WS_CHILD | SS_CENTER,
            10, 5, 760, 25, hwnd, NULL, NULL, NULL);

        // Symbol
        CreateWindowA("STATIC", "Symbol:", WS_VISIBLE | WS_CHILD, 10, 40, 50, 20, hwnd, 0, 0, 0);
        hComboSymbol = CreateWindowA("COMBOBOX", "", WS_VISIBLE | WS_CHILD | CBS_DROPDOWNLIST,
            10, 60, 60, 100, hwnd, (HMENU)100, 0, 0);
        if (hComboSymbol) {
            SendMessageA(hComboSymbol, CB_ADDSTRING, 0, (LPARAM)"H");
            SendMessageA(hComboSymbol, CB_ADDSTRING, 0, (LPARAM)"e");
            SendMessageA(hComboSymbol, CB_ADDSTRING, 0, (LPARAM)"l");
            SendMessageA(hComboSymbol, CB_ADDSTRING, 0, (LPARAM)"o");
            SendMessageA(hComboSymbol, CB_ADDSTRING, 0, (LPARAM)"W");
            SendMessageA(hComboSymbol, CB_ADDSTRING, 0, (LPARAM)"r");
            SendMessageA(hComboSymbol, CB_ADDSTRING, 0, (LPARAM)"d");
            SendMessageA(hComboSymbol, CB_SETCURSEL, 0, 0);
        }

        // Font
        CreateWindowA("STATIC", "Font:", WS_VISIBLE | WS_CHILD, 80, 40, 40, 20, hwnd, 0, 0, 0);
        hComboFont = CreateWindowA("COMBOBOX", "", WS_VISIBLE | WS_CHILD | CBS_DROPDOWNLIST,
            80, 60, 100, 100, hwnd, (HMENU)101, 0, 0);
        if (hComboFont) {
            SendMessageA(hComboFont, CB_ADDSTRING, 0, (LPARAM)"Arial");
            SendMessageA(hComboFont, CB_ADDSTRING, 0, (LPARAM)"Times");
            SendMessageA(hComboFont, CB_ADDSTRING, 0, (LPARAM)"Courier");
            SendMessageA(hComboFont, CB_SETCURSEL, 0, 0);
        }

        // Size
        CreateWindowA("STATIC", "Size:", WS_VISIBLE | WS_CHILD, 190, 40, 40, 20, hwnd, 0, 0, 0);
        hComboSize = CreateWindowA("COMBOBOX", "", WS_VISIBLE | WS_CHILD | CBS_DROPDOWNLIST,
            190, 60, 60, 100, hwnd, (HMENU)102, 0, 0);
        if (hComboSize) {
            SendMessageA(hComboSize, CB_ADDSTRING, 0, (LPARAM)"10");
            SendMessageA(hComboSize, CB_ADDSTRING, 0, (LPARAM)"12");
            SendMessageA(hComboSize, CB_ADDSTRING, 0, (LPARAM)"14");
            SendMessageA(hComboSize, CB_SETCURSEL, 1, 0);
        }

        // X
        CreateWindowA("STATIC", "X:", WS_VISIBLE | WS_CHILD, 10, 95, 20, 20, hwnd, 0, 0, 0);
        hEditX = CreateWindowA("EDIT", "10", WS_VISIBLE | WS_CHILD | WS_BORDER | ES_NUMBER,
            30, 95, 50, 22, hwnd, (HMENU)103, 0, 0);

        // Y
        CreateWindowA("STATIC", "Y:", WS_VISIBLE | WS_CHILD, 90, 95, 20, 20, hwnd, 0, 0, 0);
        hEditY = CreateWindowA("EDIT", "10", WS_VISIBLE | WS_CHILD | WS_BORDER | ES_NUMBER,
            110, 95, 50, 22, hwnd, (HMENU)104, 0, 0);

        // Add button
        CreateWindowA("BUTTON", "Add Point", WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON,
            180, 93, 100, 25, hwnd, (HMENU)1, 0, 0);

        // Clear button
        CreateWindowA("BUTTON", "Clear", WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON,
            10, 130, 80, 25, hwnd, (HMENU)2, 0, 0);

        // Info
        CreateWindowA("STATIC", "Info:", WS_VISIBLE | WS_CHILD, 320, 40, 40, 20, hwnd, 0, 0, 0);
        hEditInfo = CreateWindowA("EDIT", "", WS_VISIBLE | WS_CHILD | WS_BORDER | WS_VSCROLL |
            ES_MULTILINE | ES_READONLY,
            320, 60, 250, 150, hwnd, 0, 0, 0);

        // Stats
        CreateWindowA("STATIC", "Stats:", WS_VISIBLE | WS_CHILD, 10, 165, 40, 20, hwnd, 0, 0, 0);
        hEditStats = CreateWindowA("EDIT", "", WS_VISIBLE | WS_CHILD | WS_BORDER | WS_VSCROLL |
            ES_MULTILINE | ES_READONLY,
            10, 185, 560, 80, hwnd, 0, 0, 0);

        factory = new CharacterFactory();
        render = new TextRender();
        UpdateAll();
        return 0;
    }

    case WM_COMMAND:
        if (LOWORD(wp) == 1) OnAdd();
        else if (LOWORD(wp) == 2) OnClear();
        return 0;

    case WM_DESTROY:
        delete factory;
        delete render;
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProcA(hwnd, msg, wp, lp);
}

// ==================== MAIN ====================

int WINAPI WinMain(HINSTANCE hInst, HINSTANCE, LPSTR, int show) {
    WNDCLASSA wc = { 0 };
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInst;
    wc.lpszClassName = "Flyweight";
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);

    if (!RegisterClassA(&wc)) {
        MessageBoxA(NULL, "RegisterClass failed!", "Error", MB_ICONERROR);
        return 0;
    }

    HWND hwnd = CreateWindowExA(0, "Flyweight", "Flyweight Pattern Demo", WS_OVERLAPPEDWINDOW,
        100, 100, 600, 320, NULL, NULL, NULL, NULL);
    if (!hwnd) {
        MessageBoxA(NULL, "CreateWindow failed!", "Error", MB_ICONERROR);
        return 0;
    }

    ShowWindow(hwnd, show);

    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return 0;
}