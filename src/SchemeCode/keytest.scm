
(define keysymbol->int (lambda (p)
    (enum-value (type "System.Windows.Forms.Keys") p)))

(define int->key (lambda (i)
    (integer->enum (type "System.Windows.Forms.Keys") i)))

(define wmgr (make-object))

(add-local! wmgr 'x 0)
(add-local! wmgr 'y 0)
(add-local! wmgr 'gridx 20)
(add-local! wmgr 'gridy 20)
(add-local! wmgr 'xscale 20)
(add-local! wmgr 'yscale 20)
(add-local! wmgr 'xorigin 5)
(add-local! wmgr 'yorigin 5)
(add-local! wmgr 'xcs 16)
(add-local! wmgr 'ycs 16)
(add-local! wmgr 'xco 2)
(add-local! wmgr 'yco 2)
(add-local! wmgr 'window #f)

(set-handler! wmgr (mlambda #sig(set-origin! . x y)
    (set! (local xorigin) x)
    (set! (local yorigin) y)
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(set-scale! . x y)
    (set! (local xscale) x)
    (set! (local yscale) y)
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(set-size! . x y)
    (if (>= (local x) x) (set! (local x) (- x 1)))
    (if (>= (local y) y) (set! (local y) (- y 1)))
    (set! (local gridx) x)
    (set! (local gridy) y)
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(set-cursor-size! . x y)
    (set! (local xcs) x)
    (set! (local ycs) y)
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(set-cursor-offset! . x y)
    (set! (local xco) x)
    (set! (local yco) y)
    (post! (self) #msg(redraw))))

(define range-limit (lambda (lo hi x)
    (if (< x lo) lo (if (>= x hi) hi x))))

(set-handler! wmgr (mlambda #sig(move-to! . x y)
    (set! (local x) (range-limit 0 (local gridx) x))
    (set! (local y) (range-limit 0 (local gridy) y))
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(redraw) #t))

(post! wmgr #msg(init))
(post! wmgr #msg(move-to! . x 8 y 17))

(define with-accumulator (lambda (proc)
    (let* (
        (result (cons 'dummy '()))
        (ptr result)
        (add! (lambda (x)
            (set-cdr! ptr (cons x (cdr ptr)))
            (set! ptr (cdr ptr)))))
      (proc add!)
      (cdr result))))

(define for (lambda (s e proc)
    (let loop ((i s))
      (if (= i e) #t
        (begin (proc i) (loop (+ i 1)))))))

(define grid-drawing (lambda (gridx gridy xscale yscale xorigin yorigin)
    `(begin
      ,@(with-accumulator (lambda (add!)
          (for 0 (+ gridx 1) (lambda (i)
              (add! `(line ,(+ xorigin (* xscale i)) ,yorigin ,(+ xorigin (* xscale i)) ,(+ yorigin (* yscale gridy 1))))))
          (for 0 (+ gridy 1) (lambda (i)
              (add! `(line ,xorigin ,(+ yorigin (* yscale i)) ,(+ xorigin (* xscale gridx)) ,(+ yorigin (* yscale i)))))))))))

(define cursor-drawing (lambda (x y xscale yscale xorigin yorigin xco yco xcs ycs)
    `(clip-inside (rect ,(+ (* x xscale) xorigin xco) ,(+ (* y yscale) yorigin yco) ,(+ (* x xscale) xorigin xco xcs) ,(+ (* y yscale) yorigin yco ycs))
      (clear (color Red)))))

(set-handler! wmgr (mlambda #sig(set-window! . window)
    (set! (local window) window)))

(set-handler! wmgr (mlambda #sig(redraw)
    (post! (local window) #msg(draw . drawing
        `(begin
          (clear (color White))
          ,(grid-drawing (local gridx) (local gridy) (local xscale) (local yscale) (local xorigin) (local yorigin))
          ,(cursor-drawing (local x) (local y) (local xscale) (local yscale) (local xorigin) (local yorigin) (local xco) (local yco) (local xcs) (local ycs)))))))

(set-handler! wmgr (mlambda #sig(move-up!)
    (if (> (local y) 0) (set! (local y) (- (local y) 1)))
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(move-down!)
    (if (< (local y) (- (local gridy) 1)) (set! (local y) (+ (local y) 1)))
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(move-left!)
    (if (> (local x) 0) (set! (local x) (- (local x) 1)))
    (post! (self) #msg(redraw))))

(set-handler! wmgr (mlambda #sig(move-right!)
    (if (< (local x) (- (local gridx) 1)) (set! (local x) (+ (local x) 1)))
    (post! (self) #msg(redraw))))

(set-handler! wmgr
  (let (
      (k_up (enum-value (type "System.Windows.Forms.Keys") 'Up))
      (k_down (enum-value (type "System.Windows.Forms.Keys") 'Down))
      (k_left (enum-value (type "System.Windows.Forms.Keys") 'Left))
      (k_right (enum-value (type "System.Windows.Forms.Keys") 'Right)))
    (mlambda #sig(key-down . kdata keydata)
      (let* (
          (k (enum->integer keydata)))
        (cond
          ((= k k_up) (post! (self) #msg(move-up!)))
          ((= k k_down) (post! (self) #msg(move-down!)))
          ((= k k_left) (post! (self) #msg(move-left!)))
          ((= k k_right) (post! (self) #msg(move-right!)))
          (else #f))))))

(define cw (make-window-obj 1024 768))

(post! wmgr #msg(set-window! . window cw))

(post! wmgr #msg(redraw))

(post! cw #msg(set-dest . k wmgr kdata #t))
