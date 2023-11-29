import sampleURL from './sample.txt?url'

import { convert_text } from './lib/Program.js';

const STORAGE_KEY = 'SeqSharp_Callstack';

window.addEventListener('load', function() {
    var input = document.getElementById('input');
    input.addEventListener('input', apply_convert);

    var sample = document.getElementById('sample');
    sample.addEventListener('click', apply_sample);

    input.value = localStorage.getItem(STORAGE_KEY);

    if (!input.value)
       apply_sample();
    else
       apply_convert();
});

function apply_convert() {
    var text = document.getElementById('input').value;
    localStorage.setItem(STORAGE_KEY, text);
    document.getElementById('output').innerHTML = convert_text(text);
}

function apply_sample() {
    fetch(sampleURL)
        .then(response => response.text())
        .then(text => {
            var input = document.getElementById('input');
            input.value = text;
            apply_convert();
        });
}
