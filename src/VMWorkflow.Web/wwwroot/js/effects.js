// VM Workflow — Premium Effects

window.VmEffects = {
    // Particle canvas background for auth pages
    initParticles: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        let w = canvas.width = canvas.parentElement.offsetWidth;
        let h = canvas.height = canvas.parentElement.offsetHeight;

        const particles = [];
        const count = 50;

        for (let i = 0; i < count; i++) {
            particles.push({
                x: Math.random() * w,
                y: Math.random() * h,
                r: Math.random() * 2 + 0.5,
                dx: (Math.random() - 0.5) * 0.4,
                dy: (Math.random() - 0.5) * 0.4,
                opacity: Math.random() * 0.3 + 0.1
            });
        }

        function draw() {
            ctx.clearRect(0, 0, w, h);
            particles.forEach(p => {
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
                ctx.fillStyle = `rgba(255, 255, 255, ${p.opacity})`;
                ctx.fill();

                p.x += p.dx;
                p.y += p.dy;
                if (p.x < 0 || p.x > w) p.dx *= -1;
                if (p.y < 0 || p.y > h) p.dy *= -1;
            });

            // Draw lines between close particles
            for (let i = 0; i < particles.length; i++) {
                for (let j = i + 1; j < particles.length; j++) {
                    const dx = particles[i].x - particles[j].x;
                    const dy = particles[i].y - particles[j].y;
                    const dist = Math.sqrt(dx * dx + dy * dy);
                    if (dist < 120) {
                        ctx.beginPath();
                        ctx.moveTo(particles[i].x, particles[i].y);
                        ctx.lineTo(particles[j].x, particles[j].y);
                        ctx.strokeStyle = `rgba(255, 255, 255, ${0.06 * (1 - dist / 120)})`;
                        ctx.lineWidth = 0.5;
                        ctx.stroke();
                    }
                }
            }
            requestAnimationFrame(draw);
        }
        draw();

        window.addEventListener('resize', () => {
            w = canvas.width = canvas.parentElement.offsetWidth;
            h = canvas.height = canvas.parentElement.offsetHeight;
        });
    },

    // Parallax mouse movement for auth card
    initParallax: function (elementId) {
        const el = document.getElementById(elementId);
        if (!el) return;

        document.addEventListener('mousemove', (e) => {
            const x = (e.clientX / window.innerWidth - 0.5) * 12;
            const y = (e.clientY / window.innerHeight - 0.5) * 12;
            el.style.transform = `perspective(1000px) rotateY(${x}deg) rotateX(${-y}deg) translateZ(10px)`;
        });

        document.addEventListener('mouseleave', () => {
            el.style.transform = 'perspective(1000px) rotateY(0deg) rotateX(0deg) translateZ(0px)';
        });
    },

    // Blob parallax on mouse (subtle)
    initBlobParallax: function () {
        const blobs = document.querySelectorAll('.blob');
        if (!blobs.length) return;

        document.addEventListener('mousemove', (e) => {
            const x = (e.clientX / window.innerWidth - 0.5);
            const y = (e.clientY / window.innerHeight - 0.5);
            blobs.forEach((blob, i) => {
                const speed = (i + 1) * 15;
                blob.style.transform = `translate(${x * speed}px, ${y * speed}px)`;
            });
        });
    }
};
