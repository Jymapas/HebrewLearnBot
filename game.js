"use strict";

const names = ["אֶפֶס", "אַחַת", "שתַיִים", "שָׁלוֹשׁ", "אַרְבַּע", "חָמֵשׁ", "שֵׁשׁ", "שֶׁבַע", "שְׁמוֹנֶה", "תֵּשַׁע", "עֶשֶׂר"];
const digits = document.querySelector("#digits");
const words = document.querySelector("#words");
const feedback = document.querySelector("#feedback");
const next = document.querySelector("#next");
const limit = document.querySelector("#limit");
const progress = document.querySelector("progress");
let deck, matched, selected, missTimer;

function shuffle(values) {
    const result = [...values];
    for (let i = result.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [result[i], result[j]] = [result[j], result[i]];
    }
    return result;
}

function start() {
    clearTimeout(missTimer);
    missTimer = null;
    selected = null;
    matched = 0;
    progress.max = Number(limit.value);
    deck = [];
    for (let i = 0; i < progress.max; i += names.length) {
        deck.push(...shuffle(names.keys()));
    }
    next.hidden = true;
    feedback.textContent = "Выбери любую карточку.";
    updateProgress();
    const opening = deck.splice(0, 4);
    for (const column of [digits, words]) {
        column.replaceChildren(...shuffle(opening).map(value => makeTile(column, value)));
    }
}

function updateProgress() {
    progress.value = matched;
    document.querySelector("#count").textContent = `Собрано · ${matched} / ${progress.max}`;
}

function makeTile(column, value) {
    const tile = document.createElement("button");
    tile.className = "tile";
    tile.dataset.value = value;
    tile.setAttribute("aria-pressed", "false");
    tile.textContent = value;
    if (column === words) {
        tile.lang = "he";
        tile.dir = "rtl";
        tile.textContent = names[value];
    }
    tile.addEventListener("click", () => choose(tile));
    return tile;
}

function refill() {
    const solved = [...digits.querySelectorAll(":disabled")];
    if (solved.length < 2 || !deck.length) return;
    const active = new Set([...digits.querySelectorAll(":enabled")].map(tile => Number(tile.dataset.value)));
    const index = deck.findIndex(value => !active.has(value));
    if (index < 0) return;
    const [value] = deck.splice(index, 1);
    // Освобождаем одну решённую пару. Оставшаяся дождётся следующего совпадения.
    const previous = solved.find(tile => Number(tile.dataset.value) === value) ?? shuffle(solved)[0];
    const word = words.querySelector(`[data-value="${previous.dataset.value}"]`);
    previous.replaceWith(makeTile(digits, value));
    word.replaceWith(makeTile(words, value));
}

function choose(tile) {
    if (missTimer || tile.disabled) return;
    const first = selected;
    if (first) first.setAttribute("aria-pressed", "false");
    selected = null;
    if (first === tile) {
        feedback.textContent = "Выбери любую карточку.";
        return;
    }
    if (!first || first.parentElement === tile.parentElement) {
        selected = tile;
        tile.setAttribute("aria-pressed", "true");
        feedback.textContent = tile.parentElement === digits ? "Теперь выбери слово на иврите." : "Теперь выбери число.";
        return;
    }
    if (first.dataset.value !== tile.dataset.value) {
        feedback.textContent = "Эти карточки из разных пар. Попробуй ещё раз.";
        first.classList.add("miss");
        tile.classList.add("miss");
        missTimer = setTimeout(() => {
            first.classList.remove("miss");
            tile.classList.remove("miss");
            missTimer = null;
        }, 580);
        return;
    }
    first.disabled = tile.disabled = true;
    matched++;
    updateProgress();
    feedback.textContent = "Верно! Выбери следующую пару.";
    if (matched === progress.max) {
        feedback.textContent = "Тренировка завершена!";
        next.hidden = false;
        next.focus();
    } else {
        refill();
    }
}

limit.addEventListener("change", start);
next.addEventListener("click", () => {
    start();
    digits.firstElementChild.focus();
});

start();
