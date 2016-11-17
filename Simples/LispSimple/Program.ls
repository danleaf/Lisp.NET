(defvar *db* ())
;"fsdfsfsdfsf
(defmacro with-gensyms ((&rest names) &body body)
  `(let ,(loop for n in names collect `(,n (gensym)))
     ,@body))

(defun make-cd (title artist rating ripped)
  (list :title title :artist artist :rating rating :ripped ripped))

(defun add-record (cd) (push cd *db*))

(defun dump-db () (format t "~{~{~a: ~10t~a~%~}~%~}" *db*))

(defun prompt-read (prompt)
  (format *query-io* "~a: " prompt)
  (force-output *query-io*)
  (read-line *query-io*))

(defun prompt-for-cd()
  (make-cd
   (prompt-read "Title")
   (prompt-read "Artist")
   (or (parse-integer (prompt-read "Rating") :junk-allowed t) 0)
   (y-or-n-p "Ripped [y/n]: ")))

(defun add-cds ()
  (loop (add-record (prompt-for-cd))
     (if (not (y-or-n-p "Another? [y/n]: ")) (return))))

(defun save-db (filename)
  (with-open-file (out filename 
		       :direction :output 
		       :if-exists :supersede)
    (with-standard-io-syntax
      (print *db* out))))

(defun load-db (filename)
  (with-open-file (in filename)
    (with-standard-io-syntax
      (setf *db* (read in)))))

(defun load-defdb () (load-db "cd.db"))

(defun save-defdb () (save-db "cd.db"))

(defun select (selector) (remove-if-not selector *db*))

(defun make-cmp-exp (field value) `(equal (getf cd ,field) ,value))

(defun make-cmp-list (fields)
  (loop while fields
       collecting (make-cmp-exp (pop fields) (pop fields))))

(defmacro where (&rest clauses)
  `#'(lambda (cd) (and ,@(make-cmp-list clauses))))

(defun make-updt-exp (field value) `(setf (getf cd ,field) ,value))

(defun make-updt-list (fields)
  (loop while fields
       collecting (make-updt-exp (pop fields) (pop fields))))

(defmacro updator (selector clause)
  `#'(lambda (cd) (when (funcall ,selector cd) ,@(make-updt-list clause)) cd))

(defmacro update (selector &rest clause)
  `(setf *db* 
	 (mapcar (updator ,selector ,clause) *db*)))

(defvar *test-name* nil)

(defun report-result (result form)
  (format t "~:[FAIL~;OK~] ... ~a: ~a~%" result *test-name* form)
  result)

(defmacro combine-results (&body forms)
  (with-gensyms (result)
    `(let ((,result t))
       ,@(loop for f in forms collect `(unless ,f (setf ,result nil)))
       ,result)))

(defmacro check (&body forms)
  `(combine-results
    ,@(loop for f in forms collect `(report-result ,f ',f))))

(defmacro deftest (name param &body body)
  `(defun ,name ,param 
     (let ((*test-name* (append *test-name* (list ',name))))
       ,@body)))

(defun erase (l idx)
  (if (= idx 0)
    (cdr l)
    (cons (car l) (erase (cdr l) (- idx 1)))))

(defun pinjie (l)
  (append (cdr l) (list (car l))))

(defun baoshu-1 (l idx)
  (when (> (length l) 1)
    (if (= idx 3)
	(progn (format t "~a~%" (cdr l))
	       (baoshu-1 (cdr l) 1))
	(baoshu-1 (append (cdr l) (list (car l))) (+ idx 1)))))
      
(defun baoshu (l)
  (baoshu-1 l 1))
