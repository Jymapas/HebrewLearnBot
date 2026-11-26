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
const results = document.querySelector("#results");
const columns = document.querySelector(".columns");
const returnDelay = 5000;
let cards, deck, matched, mistakes, selected, missedTiles, missTimer, refillTimer;
let repeatAfter, previousWordRow, lastMatchAt;

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
    missedTiles = [];
    clearTimeout(refillTimer);
    repeatAfter = new Map();
    previousWordRow = new Map();
    lastMatchAt = 0;
    mistakes = 0;
    results.hidden = true;
    columns.hidden = false;
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
    const translations = shuffle(opening);
    if (translations.every((value, row) => value === opening[row])) {
        translations.push(translations.shift());
    }
    prompts.replaceChildren(...opening.map(value => makeTile(prompts, value)));
    words.replaceChildren(...translations.map(value => makeTile(words, value)));
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
    clearTimeout(refillTimer);
    if (!deck.length) return;
    const emptyPrompts = [...prompts.querySelectorAll(":disabled")];
    if (!emptyPrompts.length) return;

    const now = performance.now();
    const active = new Set([...prompts.querySelectorAll(":enabled")].map(tile => Number(tile.dataset.value)));
    const availableAt = value => repeatAfter.get(value) ?? 0;
    const candidates = deck.filter(value => !active.has(value));
    if (!candidates.length) return;
    const slotReady = emptyPrompts.length === 1 ? lastMatchAt + returnDelay : 0;
    const readyAt = Math.max(slotReady, Math.min(...candidates.map(availableAt)));
    if (readyAt > now) {
        refillTimer = setTimeout(refill, Math.ceil(readyAt - now));
        return;
    }

    const value = candidates.find(candidate => availableAt(candidate) <= now);
    deck.splice(deck.indexOf(value), 1);
    const prompt = shuffle(emptyPrompts)[0];
    const row = [...prompts.children].indexOf(prompt);
    const oldWordRow = previousWordRow.get(row);
    const wordSlots = [...words.children];
    const emptyWords = shuffle(wordSlots.filter(tile => tile.disabled));
    const hole = emptyWords[0];
    // Даже при одном свободном месте меняем соответствие строк. Выбранная карточка остаётся на месте.
    const targets = wordSlots.filter((tile, index) => index !== oldWordRow && tile !== selected);
    const target = shuffle(targets)[0];
    if (!target.disabled) {
        target.replaceWith(makeTile(words, value));
        hole.replaceWith(target);
    } else {
        target.replaceWith(makeTile(words, value));
    }
    prompt.replaceWith(makeTile(prompts, value));
    refill();
}

function clearMiss() {
    clearTimeout(missTimer);
    for (const tile of missedTiles) tile.classList.remove("miss");
    missedTiles = [];
}

function choose(tile) {
    if (tile.disabled) return;
    clearMiss();
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
        mistakes++;
        missedTiles = [first, tile];
        first.classList.add("miss");
        tile.classList.add("miss");
        missTimer = setTimeout(clearMiss, 580);
        return;
    }
    const prompt = first.parentElement === prompts ? first : tile;
    const word = first.parentElement === words ? first : tile;
    lastMatchAt = performance.now();
    repeatAfter.set(Number(tile.dataset.value), lastMatchAt + returnDelay);
    previousWordRow.set([...prompts.children].indexOf(prompt), [...words.children].indexOf(word));
    first.disabled = tile.disabled = true;
    matched++;
    updateProgress();
    const note = cards[Number(tile.dataset.value)].note;
    feedback.textContent = note ? `Верно! ${note}` : "Верно! Выбери следующую пару.";
    if (matched === progress.max) {
        feedback.textContent = note ? `Тренировка завершена! ${note}` : "Тренировка завершена!";
        columns.hidden = true;
        results.hidden = false;
        document.querySelector("#correct").textContent = matched;
        document.querySelector("#errors").textContent = mistakes;
        document.querySelector("#accuracy").textContent = Math.round(100 * matched / (matched + mistakes)) + "%";
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
