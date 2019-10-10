<?php
/**
 * jui_filter_rules, helper class for jquery.jui_filter_rules plugin, handles AJAX requests.
 **/
class jui_filter_rules {

	/** @var object Database connection */
	private $conn;
	/** @var bool Use prepared statements or not */
	private $usePreparedStatements;
	/** @var string Prepared statements placeholder type ("question_mark" or "numbered") */
	private $pst_placeholder;
	/** @var string RDBMS in use (one of "ADODB", "POSTGRES") */
	private $rdbms;
	/**
	 * @var array last_error
	 *
	 * array(
	 *    'element_rule_id' => 'the id of rule li element',
	 *    'error_message' => 'error message'
	 * )
	 *
	 */
	private $last_error;

	/**
	 * @param object $dbcon database connection
	 * @param bool $use_ps use prepared statements or not
	 * @param string $pst_placeholder // one of "question_mark" (?), "numbered" ($1, $2, ...)
	 * @param string $db_type rdbms in use (one of "ADODB", "MYSQL", "MYSQLi", "MYSQL_PDO", "POSTGRES")
	 */
	public function __construct($dbcon, $use_ps, $pst_placeholder, $db_type) {
		$this->conn = $dbcon;
		$this->usePreparedStatements = $use_ps;
		$this->rdbms = $db_type;
		$this->pst_placeholder = $pst_placeholder;
		$this->last_error = array(
			'element_rule_id' => null,
			'error_message' => null
		);
	}

	public function get_last_error() {
		return $this->last_error;
	}

	/**
	 * Parse rules array from given JSON object and returns WHERE SQL clause and bind params array (used on prepared statements)
	 *
	 * @param array $a_rules The rules array
	 * @param bool $is_group If current rule belogns to group (except first group)
	 * @return array
	 */
	public function parse_rules($a_rules, $is_group = false) {
		static $sql;
		static $bind_params = array();
		static $bind_param_index = 1;
		if(is_null($sql)) {
			$sql = 'WHERE ';
		}
		$a_len = count($a_rules);

		foreach($a_rules as $i => $rule) {
			if(!isset($rule['condition'][0])) {
				$sql .= PHP_EOL;
				$sql .= ($is_group && $i == 0 ? '(' : '');
				$sql .= $rule['condition']['field'];
				$sql .= $this->create_operator_sql($rule['condition']['operator']);

				$filter_value_conversion_server_side = array_key_exists("filter_value_conversion_server_side", $rule) ? $rule['filter_value_conversion_server_side'] : null;
				$filter_value = array_key_exists("filterValue", $rule['condition']) ? $rule['condition']['filterValue'] : null;

				$filter_value_sql = $this->create_filter_value_sql($rule['condition']['filterType'],
					$rule['condition']['operator'],
					$filter_value,
					$filter_value_conversion_server_side,
					$rule['element_rule_id']);

				if($this->usePreparedStatements) {
					if(!in_array($rule['condition']['operator'], array("is_null", "is_not_null"))) {
						if(in_array($rule['condition']['operator'], array("in", "not_in"))) {
							$sql .= '(';
							$filter_value_len = count($filter_value);
							for($v = 0; $v < $filter_value_len; $v++) {
								switch($this->pst_placeholder) {
									case "question_mark":
										$sql .= '?';
										break;
									case "numbered":
										$sql .= '$' . $bind_param_index;
										$bind_param_index++;
								}
								if($v < $filter_value_len - 1) {
									$sql .= ',';
								}
								array_push($bind_params, $filter_value[$v]);
							}
							$sql .= ')';
						} else {
							switch($this->pst_placeholder) {
								case "question_mark":
									$sql .= '?';
									break;
								case "numbered":
									$sql .= '$' . $bind_param_index;
									$bind_param_index++;
							}
							array_push($bind_params, $filter_value_sql);
						}
					}
				} else {
					$sql .= $filter_value_sql;
				}

			} else {
				$this->parse_rules($rule['condition'], true);
			}
			$sql .= ($i < $a_len - 1 ? ' ' . $rule['logical_operator'] : '');
			$sql .= ($is_group && $i == $a_len - 1 ? ')' : '');
		}
		return array('sql' => $sql, 'bind_params' => $bind_params);
	}

	/**
	 * Return current rule filter value as a string suitable for SQL WHERE clause
	 *
	 * @param string $filter_type (one of "text", "number", "date" - see documentation)
	 * @param string $operator_type (see documentation for available operators)
	 * @param array|null $a_values the values array
	 * @param array|null $filter_value_conversion_server_side
	 * @param string $element_rule_id
	 * @return string
	 */
	private function create_filter_value_sql($filter_type, $operator_type, $a_values, $filter_value_conversion_server_side, $element_rule_id) {
		$conn = $this->conn;
		$res = '';
		$vlen = count($a_values);
		if($vlen == 0) {
			if(in_array($operator_type, array("is_empty", "is_not_empty"))) {
				$res = "''";
			}
		} else {

			// apply filter value conversion (if any)
			if(is_array($filter_value_conversion_server_side)) {
				$function_name = $filter_value_conversion_server_side['function_name'];
				$args = $filter_value_conversion_server_side['args'];
				$arg_len = count($args);

				for($i = 0; $i < $vlen; $i++) {
					// create arguments values for this filter value
					$conversion_args = array();
					for($a = 0; $a < $arg_len; $a++) {
						if(array_key_exists("filter_value", $args[$a])) {
							array_push($conversion_args, $a_values[$i]);
						}
						if(array_key_exists("value", $args[$a])) {
							array_push($conversion_args, $args[$a]["value"]);
						}
					}
					// execute user function and assign return value to filter value
					try {
						$a_values[$i] = call_user_func_array($function_name, $conversion_args);
					} catch(Exception $e) {
						$this->last_error = array(
							'element_rule_id' => $element_rule_id,
							'error_message' => $e->getMessage()
						);
						break;
					}

				}
			}

			if($this->usePreparedStatements) {
				if(in_array($operator_type, array("equal", "not_equal", "less", "not_equal", "less_or_equal", "greater", "greater_or_equal"))) {
					$res = $a_values[0];
				} else if(in_array($operator_type, array("begins_with", "not_begins_with"))) {
					$res = $a_values[0] . '%';
				} else if(in_array($operator_type, array("contains", "not_contains"))) {
					$res = '%' . $a_values[0] . '%';
				} else if(in_array($operator_type, array("ends_with", "not_ends_with"))) {
					$res = '%' . $a_values[0];
				} else if(in_array($operator_type, array("in", "not_in"))) {
					for($i = 0; $i < $vlen; $i++) {
						$res .= ($i == 0 ? '(' : '');
						$res .= $a_values[$i];
						$res .= ($i < $vlen - 1 ? ',' : ')');
					}
				}
			} else {
				if(in_array($operator_type, array("equal", "not_equal", "less", "not_equal", "less_or_equal", "greater", "greater_or_equal"))) {
					$res = ($filter_type == "number" ? $a_values[0] : $this->safe_sql($a_values[0]));
				} else if(in_array($operator_type, array("begins_with", "not_begins_with"))) {
					$res = $this->safe_sql($a_values[0] . '%');
				} else if(in_array($operator_type, array("contains", "not_contains"))) {
					$res = $this->safe_sql('%' . $a_values[0] . '%');
				} else if(in_array($operator_type, array("ends_with", "not_ends_with"))) {
					$res = $this->safe_sql('%' . $a_values[0]);
				} else if(in_array($operator_type, array("in", "not_in"))) {
					for($i = 0; $i < $vlen; $i++) {
						$res .= ($i == 0 ? '(' : '');
						$res .= ($filter_type == "number" ? $a_values[$i] : $this->safe_sql($a_values[$i]));
						$res .= ($i < $vlen - 1 ? ',' : ')');
					}
				}
			}
		}
		return $res;
	}

	/**
	 * Create rule operator SQL substring
	 *
	 * @param string $operator_type
	 * @return string
	 */
	private function create_operator_sql($operator_type) {
		$operator = '';
		switch($operator_type) {
			case 'equal':
				$operator = '=';
				break;
			case 'not_equal':
				$operator = '!=';
				break;
			case 'in':
				$operator = 'IN';
				break;
			case 'not_in':
				$operator = 'NOT IN';
				break;
			case 'less':
				$operator = '<';
				break;
			case 'less_or_equal':
				$operator = '<=';
				break;
			case 'greater':
				$operator = '>';
				break;
			case 'greater_or_equal':
				$operator = '>=';
				break;
			case 'begins_with':
				$operator = 'LIKE';
				break;
			case 'not_begins_with':
				$operator = 'NOT LIKE';
				break;
			case 'contains':
				$operator = 'LIKE';
				break;
			case 'not_contains':
				$operator = 'NOT LIKE';
				break;
			case 'ends_with':
				$operator = 'LIKE';
				break;
			case 'not_ends_with':
				$operator = 'NOT LIKE';
				break;
			case 'is_empty':
				$operator = '=';
				break;
			case 'is_not_empty':
				$operator = '!=';
				break;
			case 'is_null':
				$operator = 'IS NULL';
				break;
			case 'is_not_null':
				$operator = 'IS NOT NULL';
				break;
		}
		$operator = ' ' . $operator . ' ';
		return $operator;
	}

	/**
	 * Returns escaped string for safe insertion in the database (in case prepared statements are NOT used)
	 *
	 * @param string $str_expr The string expression to be quoted
	 * @return string
	 */
	private function safe_sql($str_expr) {
		$conn = $this->conn;
		$rdbms = $this->rdbms;
		$res = '';
		switch($rdbms) {
			case "ADODB":
				$res = $conn->qstr($str_expr);
				break;
			case "MYSQL":
				/** \todo MYSQL not tested! */
				$res = mysql_real_escape_string($str_expr, $conn);
				break;
			case "MYSQLi":
				/** \todo MYSQLi not tested! */
				$res = mysqli_real_escape_string($conn, $str_expr);
				break;
			case "MYSQL_PDO":
				/** \todo MYSQL_PDO not tested! */
				$res = $conn->quote($str_expr);
				break;
			case "POSTGRES":
				$res = pg_escape_literal($conn, $str_expr);
				break;
		}
		return $res;
	}

}
