; runtime library

(define error (lambda args
    (throw
      (apply string-append
        (map (lambda (x)
            (if (string? x) x
              (object->string x)))
          args)))))

(define display (lambda (arg)
    (cond
      ((char? arg) (display-string! (make-string 1 arg)))
      ((string? arg) (display-string! arg))
      (else (display-string! (object->string arg))))))

(define newline (lambda () (display-string! "\r\n")))

(define write (lambda (arg) (display-string! (object->string arg))))

(define $$orfunc (lambda args
    (let loop ((args args))
      (if (null? args) #f
        (if (car args) #t (loop (cdr args)))))))

(define for-each (lambda (proc . lists)
    (let loop ((lists lists))
      (if (apply $$orfunc (map null? lists)) #t
        (begin
          (apply proc (map car lists))
          (loop (map cdr lists)))))))

(define for (lambda (s e proc)
    (if (< s e)
      (let loop ((i s))
        (if (< i e)
          (begin (proc i) (loop (+ i 1)))
          #t))
      (let loop ((i s))
        (if (> i e)
          (let* ((ii (- i 1)))
            (begin (proc ii) (loop ii)))
          #t)))))

(define with-accumulator (lambda (proc)
    (let* (
        (the-list '())
        (add! (lambda (x) (set! the-list (cons x the-list))))
        (result (lambda () (reverse the-list))))
      (proc add!)
      (result))))

(define iota (lambda (s e)
    (with-accumulator (lambda (add!) (for s e add!)))))

(define fold (lambda (next init the-list)
    (let loop (
        (state init)
        (the-list the-list))
      (if (null? the-list) state
        (loop (next state (car the-list)) (cdr the-list))))))

(define collect (lambda (proc . lists)
    (with-accumulator (lambda (add!)
        (let loop ((lists lists))
          (if (apply $$orfunc (map null? lists)) #t
            (let loop2 ((result (apply proc (map car lists))))
              (if (null? result)
                (loop (map cdr lists))
                (begin
                  (add! (car result))
                  (loop2 (cdr result)) )))))))))

(define merge (lambda (< list1 list2)
    (let loop ((list1 list1) (list2 list2) (result '()))
      (let* (
          (take1 (lambda () (loop (cdr list1) list2 (cons (car list1) result))))
          (take2 (lambda () (loop list1 (cdr list2) (cons (car list2) result)))))
        (cond
          ((and (null? list1) (null? list2)) (reverse result))
          ((null? list1) (take2))
          ((null? list2) (take1))
          ((< (car list2) (car list1)) (take2))
          (else (take1)) )))))

(define sort (lambda (< list1)
    (let* ((q (make-vector 0 #f)))
      (let loop ((list1 list1))
        (if (null? list1)
          (begin
            (vector-push-back! q #f)
            (let loop2 ()
              (cond
                ((= (vector-length q) 1) '())
                ((= (vector-length q) 2)
                  (let* ((i1 (vector-pop-front! q)))
                    (if (eq? i1 #f)
                      (vector-pop-front! q)
                      i1)))
                (else
                  (begin
                    (let* ((i1 (vector-pop-front! q)))
                      (if (eq? i1 #f)
                        (vector-push-back! q #f)
                        (let* ((i2 (vector-pop-front! q)))
                          (if (eq? i2 #f)
                            (begin
                              (vector-push-back! q i1)
                              (vector-push-back! q #f))
                            (vector-push-back! q (merge < i1 i2)) ))))
                    (loop2) )))))
          (begin
            (vector-push-back! q (cons (car list1) '()))
            (loop (cdr list1))))))))

(define append (lambda lists
    (with-accumulator (lambda (add!)
        (let loop ((lists lists))
          (if (null? lists) #t
            (let* (
                (l1 (car lists))
                (lists (cdr lists)))
              (let loop2 ((l1 l1))
                (if (null? l1) (loop lists)
                  (begin
                    (add! (car l1))
                    (loop2 (cdr l1))))))))))))

(define shuffle-vector! (lambda (random-int v)
    (let* (
        (len (vector-length v))
        (vector-swap! (lambda (x y)
            (let* (
                (c1 (vector-ref v x))
                (c2 (vector-ref v y)))
              (vector-set! v x c2)
              (vector-set! v y c1)))))
      (for 0 len
        (lambda (x)
          (let* (
              (range (- len x))
              (y (+ x (random-int range))))
            (vector-swap! x y))))
      v)))

(define shuffle (lambda (random-int list1)
    (vector->list (shuffle-vector! random-int (list->vector list1)))))

(define $$make-symbol (lambda (i)
    (let loop (
        (i i)
        (j ""))
      (cond
        ((<= i 1) (string->symbol (string-append "c" j "r")))
        ((even? i) (loop (floor (/ i 2)) (string-append "a" j)))
        (else (loop (floor (/ i 2)) (string-append "d" j)))))))

(define $$make-body (lambda (i)
    (let loop (
        (i i)
        (k 'x))
      (cond
        ((<= i 1) `(lambda (x) ,k))
        ((even? i) (loop (floor (/ i 2)) `(car ,k)))
        (else (loop (floor (/ i 2)) `(cdr ,k)))))))

(define $$make-cxxxr (lambda (i) `(define ,($$make-symbol i) ,($$make-body i))))

(define $$make-symbol-q (lambda (i)
    (let loop (
        (i i)
        (j ""))
      (cond
        ((<= i 1) (string->symbol (string-append "c" j "r?")))
        ((even? i) (loop (floor (/ i 2)) (string-append "a" j)))
        (else (loop (floor (/ i 2)) (string-append "d" j)))))))

(define $$make-body-q (lambda (i)
    (cond
      ((= i 2) 'pair?)
      ((= i 3) 'pair?)
      (else
        (let loop (
            (i (floor (/ i 2)))
            (j '()))
          (cond
            ((<= i 1) `(lambda (x) (and (pair? x) ,@j)))
            (else (loop (floor (/ i 2)) (cons `(pair? (,($$make-symbol i) x)) j))) ))))))

(define $$make-cxxxr-q (lambda (i) `(define ,($$make-symbol-q i) ,($$make-body-q i))))

(for-each (lambda (q) (eval $$this q)) (map $$make-cxxxr (iota 4 32)))

(for-each (lambda (q) (eval $$this q)) (map $$make-cxxxr-q (iota 2 32)))

(define $$memp (lambda (eqv? obj lst)
    (let loop ((lst lst))
      (cond
        ((null? lst) #f)
        ((eqv? obj (car lst)) lst)
        (else (loop (cdr lst)))))))

(define memq (lambda (obj lst) ($$memp eq? obj lst)))
(define memv (lambda (obj lst) ($$memp eqv? obj lst)))
; (define member (lambda (obj lst) ($$memp equal? obj lst)))

(define $$assp (lambda (eqv? obj lst)
    (let loop ((lst lst))
      (cond
        ((null? lst) #f)
        ((not (caar? lst)) (throw "Invalid association list"))
        ((eqv? obj (caar lst)) (car lst))
        (else (loop (cdr lst)))))))

(define assq (lambda (obj lst) ($$assp eq? obj lst)))
(define assv (lambda (obj lst) ($$assp eqv? obj lst)))
; (define assoc (lambda (obj lst) ($$assp equal? obj lst)))

(define $$make-compare (lambda (<)
    (lambda (arg1 arg2 . args)
      (let loop ((j (cons arg1 (cons arg2 args))))
        (cond
          ((null? (cdr j)) #t)
          ((< (car j) (cadr j)) (loop (cdr j)))
          (else #f))))))

(define char<? ($$make-compare $$char<?))
(define char>? ($$make-compare $$char>?))
(define char<=? ($$make-compare $$char<=?))
(define char>=? ($$make-compare $$char>=?))
(define char=? ($$make-compare $$char=?))
(define char-ci<? ($$make-compare $$char-ci<?))
(define char-ci>? ($$make-compare $$char-ci>?))
(define char-ci<=? ($$make-compare $$char-ci<=?))
(define char-ci>=? ($$make-compare $$char-ci>=?))
(define char-ci=? ($$make-compare $$char-ci=?))
