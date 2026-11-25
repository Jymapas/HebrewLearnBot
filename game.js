"use strict";

const names = ["אֶפֶס", "אַחַת", "שְׁתַּיִם", "שָׁלוֹשׁ", "אַרְבַּע", "חָמֵשׁ", "שֵׁשׁ", "שֶׁבַע", "שְׁמוֹנֶה", "תֵּשַׁע", "עֶשֶׂר"];
const digits = document.querySelector("#digits");
const words = document.querySelector("#words");
const feedback = document.querySelector("#feedback");
const next = document.querySelector("#next");
let rounds, round, matched, selected, checking;

function shuffle(values) {
    const result = [...values];
    for (let i = result.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [result[i], result[j]] = [result[j], result[i]];
    }
    return result;
}

function start() {
    const deck = shuffle(names.keys());
    rounds = [deck.slice(0, 4), deck.slice(4, 8), deck.slice(8)];
    round = 0;
    matched = 0;
    renderRound();
}

function updateProgress() {
    document.querySelector("progress").value = matched;
    document.querySelector("#count").textContent = `Пар собрано · ${matched} / ${names.length}`;
}

function renderRound() {
    selected = null;
    checking = false;
    next.hidden = true;
    document.querySelector("#round").textContent = `Раунд ${round + 1} из ${rounds.length}`;
    feedback.textContent = "Выбери любую карточку.";
    updateProgress();
    for (const column of [digits, words]) {
        column.replaceChildren();
        for (const value of shuffle(rounds[round])) {
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
            column.append(tile);
        }
    }
}

function choose(tile) {
    if (checking || tile.disabled) return;
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
        checking = true;
        feedback.textContent = "Эти карточки из разных пар. Попробуй ещё раз.";
        first.classList.add("miss");
        tile.classList.add("miss");
        setTimeout(() => {
            first.classList.remove("miss");
            tile.classList.remove("miss");
            checking = false;
        }, 580);
        return;
    }
    first.disabled = tile.disabled = true;
    matched++;
    updateProgress();
    feedback.textContent = "Верно! Выбери следующую пару.";
    if (!digits.querySelector(":enabled")) {
        const finished = matched === names.length;
        feedback.textContent = finished ? "Все 11 пар собраны!" : "Все пары раунда собраны!";
        next.textContent = finished ? "Сыграть ещё раз" : "Следующий раунд";
        next.hidden = false;
        next.focus();
    }
}

next.addEventListener("click", () => {
    if (matched === names.length) {
        start();
    } else {
        round++;
        renderRound();
    }
    digits.firstElementChild.focus();
});

start();
