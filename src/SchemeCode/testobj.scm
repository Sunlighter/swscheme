(define a1 (make-object))

(set-handler! a1 (mlambda #msg(add . x x y y k k) (post! k #msg(result . value (+ x y)))))

(get-signatures a1)

(define a2 (with-get-from-temporary cons))

(post! a1 #msg(add . x 3 y 4 k (car a2)))

(wait-any (cdr a2))

(with-get-from-temporary (lambda (destobj getop)
    (post! a1 #msg(add . x 3 y 4 k destobj))
    (wait-any getop)))

(define t1 (lambda (x) (mcase x
      (#sig(test . x y) (list 'test-x-y x y))
      (#msg(test . x a y b z c) (list 'test-x-y-z a b c))
      (else 100))))


(define b (make-object))

(add-local! b 'value)

(set-handler! b (mlambda #sig(set . value k kdata) (set! (local value) value) (post! k #msg(done . kdata kdata))))

(set-handler! b (mlambda #sig(get . k kdata) (post! k #msg(value . value (local value) kdata kdata))))

(define b-set! (lambda (val)
    (with-get-from-temporary (lambda (destobj getop)
        (post! b #msg(set . value val k destobj kdata #f))
        (wait-any getop)
        #t))))

(define b-get (lambda ()
    (with-get-from-temporary (lambda (destobj getop)
        (post! b #msg(get . k destobj kdata #f))
        (message-ref (hashmap-ref (wait-any getop) 'result) 'value)))))

