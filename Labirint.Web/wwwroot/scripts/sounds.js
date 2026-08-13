const soundVariants = {
    bomb: ['media/baxbax.mp3'],
    score: ['media/score.mp3'],
    step: Array.from({ length: 13 }, (_, index) => `media/steps/step${index + 1}.mp3`),
    molot: ['media/molots/molot1.mp3', 'media/molots/molot2.mp3'],
};

const poolLimit = 4;
const pools = {};

const take = source => {
    const pool = pools[source] ??= [];
    const free = pool.find(audio => audio.paused || audio.ended);

    if (free) {
        return free;
    }

    const audio = new Audio(source);
    audio.preload = 'auto';

    if (pool.length < poolLimit) {
        pool.push(audio);
    }

    return audio;
};

function playSound(soundType, volume) {
    const variants = soundVariants[soundType];
    const source = variants ? variants[Math.floor(Math.random() * variants.length)] : soundType;

    const audio = take(source);
    audio.volume = volume;
    audio.currentTime = 0;
    audio.play().catch(() => { });
}
