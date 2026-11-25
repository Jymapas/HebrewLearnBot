"use strict";

const prompts = document.querySelector("#prompts");
const words = document.querySelector("#words");
const feedback = document.querySelector("#feedback");
const next = document.querySelector("#next");
const limit = document.querySelector("#limit");
const progress = document.querySelector("progress");
const topic = document.querySelector("#topic");
const transcription = document.querySelector("#transcription");
const readingKey = "hebrew-pairs.show-reading";
let cards, deck, matched, selected, missTimer;

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
    cards = topics[topic.value].cards;
    document.querySelector(".eyebrow").textContent = topics[topic.value].title;
    document.querySelector("#prompt-heading").textContent = topic.value === "numbers" ? "Число" : "По-русски";
    deck = [];
    for (let i = 0; i < progress.max; i += cards.length) {
        deck.push(...shuffle(cards.keys()).slice(0, progress.max - i));
    }
    next.hidden = true;
    feedback.textContent = "Выбери любую карточку.";
    updateProgress();
    const opening = deck.splice(0, 4);
    for (const column of [prompts, words]) {
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
    const card = cards[value];
    tile.textContent = card.ru;
    if (column === words) {
        tile.lang = "he";
        tile.dir = "rtl";
        tile.textContent = card.he;
        const hint = document.createElement("small");
        hint.className = "reading";
        hint.lang = "ru";
        hint.dir = "ltr";
        hint.textContent = card.reading;
        tile.append(hint);
    } else if (card.example) {
        const example = document.createElement("small");
        example.textContent = card.example;
        tile.append(example);
    }
    tile.addEventListener("click", () => choose(tile));
    return tile;
}

function refill() {
    const solved = [...prompts.querySelectorAll(":disabled")];
    if (solved.length < 2 || !deck.length) return;
    const active = new Set([...prompts.querySelectorAll(":enabled")].map(tile => Number(tile.dataset.value)));
    const index = deck.findIndex(value => !active.has(value));
    if (index < 0) return;
    const [value] = deck.splice(index, 1);
    // Освобождаем одну решённую пару. Оставшаяся дождётся следующего совпадения.
    const previous = solved.find(tile => Number(tile.dataset.value) === value) ?? shuffle(solved)[0];
    const word = words.querySelector(`[data-value="${previous.dataset.value}"]`);
    previous.replaceWith(makeTile(prompts, value));
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
        feedback.textContent = tile.parentElement === prompts ? "Теперь выбери слово на иврите." : "Теперь выбери пару слева.";
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
    const note = cards[Number(tile.dataset.value)].note;
    feedback.textContent = note ? `Верно! ${note}` : "Верно! Выбери следующую пару.";
    if (matched === progress.max) {
        feedback.textContent = note ? `Тренировка завершена! ${note}` : "Тренировка завершена!";
        next.hidden = false;
        next.focus();
    } else {
        refill();
    }
}

function applyReading() {
    document.body.classList.toggle("show-reading", transcription.checked);
}

try {
    transcription.checked = localStorage.getItem(readingKey) === "true";
} catch {
    // В некоторых WebView хранилище недоступно.
}
applyReading();
transcription.addEventListener("change", () => {
    applyReading();
    try {
        localStorage.setItem(readingKey, String(transcription.checked));
    } catch {
        // Настройка по-прежнему действует в текущей игре.
    }
});
topic.addEventListener("change", start);
limit.addEventListener("change", start);
next.addEventListener("click", () => {
    start();
    prompts.firstElementChild.focus();
});

start();
